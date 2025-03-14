
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Handlers.Streams;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Handlers.Logging;

namespace HttpFlvServerDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure the server port
            int port = 8080;

            // Configure the server
            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler("LSTN"))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new LoggingHandler("CONN"));
                        pipeline.AddLast("HttpServerCodec", new HttpServerCodec());
                        //pipeline.AddLast("ChunkedWriter", new ChunkedWriteHandler());
                        pipeline.AddLast("HttpFlvHandler", new HttpFlvHandler());
                    }));

                IChannel bootstrapChannel = await bootstrap.BindAsync(port);

                Console.WriteLine($"HTTP-FLV Server started on port {port}");
                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }
    }

    // Handler for HTTP-FLV requests
    public class HttpFlvHandler : SimpleChannelInboundHandler<IHttpObject>
    {
        private bool isFlvRequest = false;
        private string requestPath;

        protected override void ChannelRead0(IChannelHandlerContext ctx, IHttpObject msg)
        {
            if (msg is IHttpRequest request)
            {
                // Extract request path
                requestPath = request.Uri;

                // Check if it's a FLV request
                isFlvRequest = requestPath.EndsWith(".flv", StringComparison.OrdinalIgnoreCase);

                if (isFlvRequest)
                {
                    // Prepare response headers
                    var response = new DefaultHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK);
                    response.Headers.Set(HttpHeaderNames.ContentType, "video/x-flv");
                    response.Headers.Set(HttpHeaderNames.TransferEncoding, HttpHeaderValues.Chunked);
                    response.Headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.KeepAlive);
                    response.Headers.Set(HttpHeaderNames.CacheControl, "no-cache");

                    // Write the response headers
                    ctx.WriteAndFlushAsync(response);

                    // Start sending FLV chunks
                    StartFlvStreaming(ctx);
                }
                else
                {
                    // Return 404 for non-FLV requests
                    var response = new DefaultFullHttpResponse(
                        HttpVersion.Http11,
                        HttpResponseStatus.NotFound,
                        Unpooled.CopiedBuffer("404 Not Found - Only FLV streams are supported", Encoding.UTF8));

                    response.Headers.Set(HttpHeaderNames.ContentType, "text/plain");
                    response.Headers.Set(HttpHeaderNames.ContentLength, response.Content.ReadableBytes);

                    ctx.WriteAndFlushAsync(response);
                    ctx.CloseAsync();
                }
            }
        }

        private void StartFlvStreaming(IChannelHandlerContext ctx)
        {
            // Create and send the FLV header
            byte[] flvHeader = CreateFlvHeader();
            var headerChunk = new DefaultHttpContent(Unpooled.WrappedBuffer(flvHeader));
            ctx.WriteAndFlushAsync(headerChunk);

            // Start sending FLV chunks (simulate streaming)
            _ = SendFlvChunksAsync(ctx);
        }

        private byte[] CreateFlvHeader()
        {
            // FLV header (9 bytes) + PreviousTagSize0 (4 bytes)
            byte[] header = new byte[13];

            // FLV signature: "FLV"
            header[0] = 0x46; // F
            header[1] = 0x4C; // L
            header[2] = 0x56; // V

            // FLV version: 1
            header[3] = 0x01;

            // Flags: 5 (audio + video)
            header[4] = 0x05; // 1 (audio) + 4 (video)

            // Data offset: 9
            header[5] = 0x00;
            header[6] = 0x00;
            header[7] = 0x00;
            header[8] = 0x09;

            // PreviousTagSize0: always 0
            header[9] = 0x00;
            header[10] = 0x00;
            header[11] = 0x00;
            header[12] = 0x00;

            return header;
        }

        private async Task SendFlvChunksAsync(IChannelHandlerContext ctx)
        {
            try
            {
                // Ideally, you would read from an actual FLV file or stream
                // This example simulates streaming with generated content

                string filePath = GetFlvFilePath(requestPath);

                if (File.Exists(filePath))
                {
                    // Stream from actual file
                    await StreamFromFileAsync(ctx, filePath);
                }
                else
                {
                    // Generate sample data
                    await GenerateSampleFlvChunksAsync(ctx);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error streaming FLV: {ex.Message}");
                await ctx.CloseAsync();
            }
        }

        private string GetFlvFilePath(string requestPath)
        {
            // Extract filename from path and map to local file system
            // This is a simple implementation - you'll need to adjust for your needs
            string fileName = Path.GetFileName(requestPath);
            return Path.Combine("flv_files", fileName); // Assuming a folder named "flv_files"
        }

        private async Task StreamFromFileAsync(IChannelHandlerContext ctx, string filePath)
        {
            const int CHUNK_SIZE = 64 * 1024; // 64KB chunks

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[CHUNK_SIZE];
                int bytesRead;

                // Skip the FLV header if it exists (we already sent our own)
                if (fileStream.Length > 13)
                {
                    fileStream.Position = 13;
                }

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    // Create a buffer with exactly the bytes read
                    var content = Unpooled.CopiedBuffer(buffer, 0, bytesRead);
                    var chunk = new DefaultHttpContent(content);

                    // Write the chunk
                    await ctx.WriteAndFlushAsync(chunk);

                    // Simulate realistic streaming pace
                    await Task.Delay(100); // Adjust delay as needed
                }

                // End of stream
                //await ctx.WriteAndFlushAsync(LastHttpContent.Empty);
                await ctx.CloseAsync();
            }
        }

        private async Task GenerateSampleFlvChunksAsync(IChannelHandlerContext ctx)
        {
            // This simulates FLV streaming with generated content
            // In a real application, you would replace this with actual FLV data

            Random random = new Random();

            for (int i = 0; i < 100; i++) // Send 100 chunks
            {
                // Create a simulated FLV tag
                byte[] tag = CreateSampleFlvTag(i, random);

                var chunk = new DefaultHttpContent(Unpooled.WrappedBuffer(tag));
                await ctx.WriteAndFlushAsync(chunk);

                // Add delay to simulate realistic streaming
                await Task.Delay(100); // Adjust as needed

                // Check if the connection is still active
                if (!ctx.Channel.IsWritable)
                {
                    break;
                }
            }

            // End of stream
            //await ctx.WriteAndFlushAsync(LastHttpContent.Empty);
            await ctx.CloseAsync();
        }

        private byte[] CreateSampleFlvTag(int tagIndex, Random random)
        {
            // This creates a simulated FLV tag
            // FLV tag consists of: TagType(1) + DataSize(3) + Timestamp(3) + TimestampExtended(1) + StreamID(3) + Data(DataSize) + PreviousTagSize(4)

            bool isVideo = (tagIndex % 3 != 0); // 2/3 chance of video tag, 1/3 chance of audio tag
            byte tagType = isVideo ? (byte)0x09 : (byte)0x08; // 8 for audio, 9 for video

            // Randomize payload size between 1KB and 5KB
            int dataSize = random.Next(1024, 5 * 1024);
            byte[] data = new byte[dataSize];
            random.NextBytes(data); // Fill with random data

            // Calculate timestamp (30 fps)
            int timestamp = tagIndex * 33; // milliseconds (roughly 30 fps)

            // Create the tag
            byte[] tag = new byte[dataSize + 11 + 4]; // Tag header (11) + Data + PreviousTagSize (4)

            // Tag Type
            tag[0] = tagType;

            // Data Size (3 bytes, big-endian)
            tag[1] = (byte)((dataSize >> 16) & 0xFF);
            tag[2] = (byte)((dataSize >> 8) & 0xFF);
            tag[3] = (byte)(dataSize & 0xFF);

            // Timestamp (3 bytes, big-endian) + TimestampExtended (1 byte)
            tag[4] = (byte)((timestamp >> 16) & 0xFF);
            tag[5] = (byte)((timestamp >> 8) & 0xFF);
            tag[6] = (byte)(timestamp & 0xFF);
            tag[7] = (byte)((timestamp >> 24) & 0xFF); // Extended timestamp

            // Stream ID (3 bytes, always 0)
            tag[8] = 0;
            tag[9] = 0;
            tag[10] = 0;

            // Tag data
            Buffer.BlockCopy(data, 0, tag, 11, dataSize);

            // Previous tag size (4 bytes, big-endian)
            int previousTagSize = dataSize + 11; // header size + data size
            tag[11 + dataSize] = (byte)((previousTagSize >> 24) & 0xFF);
            tag[11 + dataSize + 1] = (byte)((previousTagSize >> 16) & 0xFF);
            tag[11 + dataSize + 2] = (byte)((previousTagSize >> 8) & 0xFF);
            tag[11 + dataSize + 3] = (byte)(previousTagSize & 0xFF);

            return tag;
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
        {
            Console.WriteLine($"Exception: {exception}");
            ctx.CloseAsync();
        }
    }
}