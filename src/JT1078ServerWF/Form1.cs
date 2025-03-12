using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using JT1078NetCore.Http;
using DotNetty.Codecs;
using JT1078NetCore.Socket;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebSocketSharp.Server;
using static JT1078NetCore.Http.WsService;
using static System.Net.WebRequestMethods;
using System;
using System.Threading.Tasks;
using JT1078NetCore.Utils;
using Microsoft.AspNetCore.Http;
using System.Web;
using DotNetty.Buffers;
using DotNetty.Codecs.Http.Cors;
using DotNetty.Codecs.Http;
using JT1078NetCore.Common;

namespace JT1078ServerWF
{
    public partial class Form1 : Form
    {
        private static ServerBootstrap bootstrap;
        private static IChannel bootstrapChannel;
        public static ConcurrentDictionary<string, IChannel> Channels = new ConcurrentDictionary<string, IChannel>();
        public static ConcurrentDictionary<string, SocketSession> Sessions = new ConcurrentDictionary<string, SocketSession>();
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var bossGroup = new MultithreadEventLoopGroup();
            var workerGroup = new MultithreadEventLoopGroup();
            bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup)
                .Channel<TcpServerSocketChannel>()
                //.Option(ChannelOption.SoBacklog, 8192)// 1024
                .Option(ChannelOption.SoRcvbuf, 0x8340000) // 4,194,304 // 65536 // 32768 // 846750
                .Option(ChannelOption.SoSndbuf, 0x8340000)
                .Option(ChannelOption.TcpNodelay, true)
                  //.Option(ChannelOption.AutoRead, true)
                  //.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                  //.Option(ChannelOption.WriteBufferHighWaterMark, 1048576)              
                  //.Option(ChannelOption.SoSndbuf, 102400)
                  //.Option(ChannelOption.SoRcvbuf, 102400)                    
                  //.Handler(new LoggingHandler(LogLevel.INFO))
                  // howen
                  .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                  {
                      IChannelPipeline pipeline = channel.Pipeline;
                      channel.Pipeline.AddLast("encoder", new ByteEncoder());
                      channel.Pipeline.AddLast("hexDecoder", new HexDecoder());
                      channel.Pipeline.AddLast("dataFilter", new SocketProcess());
                      channel.Pipeline.AddLast(new StringDecoder(), new SocketHandler());
                  }));
            new WsService().Init();
            bootstrapChannel = await bootstrap.BindAsync(2202);
            JT1078NetCore.Http.WebSocketServer webSocketServer = new JT1078NetCore.Http.WebSocketServer();
            await webSocketServer.Init();
        }
        private async Task InitHTTP()
        {
           _ = new HttpFlvServer().StartAsync(8080);
        }

        public class HttpFlvServer
        {
            private IEventLoopGroup bossGroup;
            private IEventLoopGroup workerGroup;

            public async Task StartAsync(int port)
            {
                bossGroup = new MultithreadEventLoopGroup(1);
                workerGroup = new MultithreadEventLoopGroup();

                try
                {
                    var bootstrap = new ServerBootstrap();
                    bootstrap.Group(bossGroup, workerGroup);
                    bootstrap.Channel<TcpServerSocketChannel>();
                    bootstrap.Option(ChannelOption.SoBacklog, 100);
                    bootstrap.ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // HTTP codec
                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(65536));

                        // Thêm CORS header nếu cần
                        //pipeline.AddLast(new CorsHandler());

                        // Handler xử lý HTTP request để trả về FLV stream
                        pipeline.AddLast(new HttpFlvStreamHandler());
                    }));

                    IChannel bootstrapChannel = await bootstrap.BindAsync(port);
                    Console.WriteLine($"HTTP-FLV streaming server started on port {port}");
                    while (true)
                    {
                        var flvHeader = Unpooled.Buffer(9);
                        flvHeader.WriteBytes(new byte[] { 0x46, 0x4C, 0x56, 0x01, 0x05, 0x00, 0x00, 0x00, 0x09 }); // FLV header
                        if(Global.ctx != null)
                        {
                            await Global.ctx.WriteAndFlushAsync(flvHeader);
                        }
                        
                        await Task.Delay(5000);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Error starting server: {ex.Message}");
                    //await ShutdownAsync();
                }
            }

            // ...
        }

        // HTTP handler để phục vụ FLV stream
        public class HttpFlvStreamHandler : SimpleChannelInboundHandler<IFullHttpRequest>
        {
            protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest request)
            {
                if (request.Uri.Contains("/flv"))
                {
                    // Thiết lập HTTP response
                    var response = new DefaultHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK);
                    response.Headers.Set(HttpHeaderNames.ContentType, "video/x-flv");
                    response.Headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.KeepAlive);
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "*");

                    ctx.WriteAndFlushAsync(response);

                    // Ghi FLV header
                    var flvHeader = Unpooled.Buffer(9);
                    flvHeader.WriteBytes(new byte[] { 0x46, 0x4C, 0x56, 0x01, 0x05, 0x00, 0x00, 0x00, 0x09 }); // FLV header
                    ctx.WriteAndFlushAsync(flvHeader);

                    // Bắt đầu stream FLV từ nguồn của bạn
                    StartStreamingFlv(ctx);
                }
                else if (request.Uri.Contains("/ws"))
                {
                    // Thiết lập HTTP response
                    //var response = new DefaultHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK);
                    //response.Headers.Set(HttpHeaderNames.ContentType, "video/x-flv");
                    //response.Headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.KeepAlive);
                    //response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
                    //response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "*");

                    //ctx.WriteAndFlushAsync(response);

                    // Ghi FLV header
                    var flvHeader = Unpooled.Buffer(9);
                    flvHeader.WriteBytes(new byte[] { 0x46, 0x4C, 0x56, 0x01, 0x05, 0x00, 0x00, 0x00, 0x09 }); // FLV header
                    ctx.WriteAndFlushAsync(flvHeader);

                    // Bắt đầu stream FLV từ nguồn của bạn
                    StartStreamingFlv(ctx);
                }
                else
                {
                    // Xử lý các endpoints khác nếu cần
                    ctx.WriteAndFlushAsync(new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.NotFound));
                }
            }

            private void StartStreamingFlv(IChannelHandlerContext ctx)
            {
                Global.ctx = ctx;
                // Triển khai logic để lấy dữ liệu FLV từ nguồn và gửi nó cho client
                // Đây có thể là một tệp FLV, một stream từ camera, hoặc bất kỳ nguồn nào khác

                // Ví dụ đơn giản: gửi dữ liệu FLV từ tệp
                // Task.Run(async () => {
                //    using (var fileStream = File.OpenRead("path/to/your/video.flv"))
                //    {
                //        var buffer = new byte[4096];
                //        int bytesRead;
                //        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                //        {
                //            var flvBuffer = Unpooled.WrappedBuffer(buffer, 0, bytesRead);
                //            await ctx.WriteAndFlushAsync(flvBuffer);
                //        }
                //    }
                // });
            }
        }
        //private async void InitHTTP()
        //{
        //    var builder = WebApplication.CreateBuilder();
        //    builder.Services.AddControllers();
        //    var app = builder.Build();
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //    app.UseHttpsRedirection();
        //    app.UseRouting();
        //    app.UseAuthorization();
        //    app.MapControllers();
        //    app.Run();

        //}

        private void btnHttpInit_Click(object sender, EventArgs e)
        {
            _ = InitHTTP();
            //HttpServer httpServer = new HttpServer(5002);
            //httpServer.OnGet += (sender, e) =>
            //{
            //    var req = e.Request;
            //    var res = e.Response;
            //    var path = req.RawUrl;
            //    try
            //    {
            //       if(path.StartsWith("/api/live"))
            //        {
            //            // add proxy
            //            Uri myUri = new Uri("http://www.example.com?param1=good&param2=bad");
            //            var paramQuery = HttpUtility.ParseQueryString(path);
            //            string app = paramQuery.Get("app").ToString();
            //            string imei = paramQuery.Get("imei").ToString();
            //            string ch = paramQuery.Get("ch").ToString();        
            //            string token = Guid.NewGuid().ToString();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        res.Close();
            //        ExceptionHandler.ExceptionProcess(ex);
            //    }             
                
            //};
            ////httpServer.AddWebSocketService<WsSession>("/live2");
            ////httpServer.AddWebSocketService<WsSession>("/ChatWithNyan");
            //httpServer.Start();
        }
    }
}
