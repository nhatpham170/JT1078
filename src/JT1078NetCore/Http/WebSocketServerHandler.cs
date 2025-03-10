using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using static System.Net.WebRequestMethods;
using System.Diagnostics;
using System.Text;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using JT1078NetCore.Common;
using System.Web;
using DotNetty.Common;


namespace JT1078NetCore.Http
{
    public class WebSocketServerHandler: SimpleChannelInboundHandler<object>
    {
        const string WebsocketPath = "/websocket";

        WebSocketServerHandshaker handshaker;
        sealed class MessageBody
        {
            public MessageBody(string message)
            {
                this.Message = message;
            }

            public string Message { get; }

            public string ToJsonFormat() => "{" + $"\"{nameof(MessageBody)}\" :" + "{" + $"\"{nameof(this.Message)}\"" + " :\"" + this.Message + "\"}" + "}";
        }
        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            if (msg is IFullHttpRequest request)
            {
                this.HandleHttpRequest(ctx, request);
            }
            else if (msg is WebSocketFrame frame)
            {
                this.HandleWebSocketFrame(ctx, msg as WebSocketFrame);
            }
        }
        public override void ChannelActive(IChannelHandlerContext context)
        {
            try
            {
                //Global.DictChannels[context.Channel.Id.ToString()] = context.Channel;
            }
            catch (Exception ex)
            {

                //ExceptionHandler.ExceptionProcess(ex);
            }
        }
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            try
            {
                string channelId = context.Channel.Id.ToString();
                IChannelHandlerContext tmp;
                IChannel channel;
                //Global.DictChannels.TryRemove(channelId, out channel);
            }
            catch (Exception ex)
            {
                //ExceptionHandler.ExceptionProcess(ex);
            }
            finally
            {
                base.ChannelInactive(context);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        void HandleHttpRequest(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            // Handle a bad request.
            if (!req.Result.IsSuccess)
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.BadRequest));
                return;
            }

            // Allow only GET methods.
            if (!Equals(req.Method, DotNetty.Codecs.Http.HttpMethod.Get))
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.Forbidden));
                return;
            }

            // Send the demo page and favicon.ico
            if ("/".Equals(req.Uri))
            {
                //IByteBuffer content = WebSocketServerBenchmarkPage.GetContent(GetWebSocketLocation(req));
                IByteBuffer content = Unpooled.WrappedBuffer(Encoding.ASCII.GetBytes("Xin chào"));
                IByteBuffer content2 = Unpooled.WrappedBuffer(Encoding.ASCII.GetBytes("Xin chào 2"));
                var res = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, content);

                res.Headers.Set(HttpHeaderNames.ContentType, "text/html; charset=UTF-8");
                HttpUtil.SetContentLength(res, content.ReadableBytes);

                SendHttpResponse(ctx, req, res);
                //HttpUtil.SetContentLength(res, content2.ReadableBytes);                
                //ctx.Channel.WriteAndFlushAsync(res);
                byte[] json = Encoding.UTF8.GetBytes(NewMessage().ToJsonFormat());
                this.WriteResponse(ctx, Unpooled.WrappedBuffer(json), TypeJson, JsonClheaderValue);

                return;
            }
            if ("/favicon.ico".Equals(req.Uri))
            {
                var res = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.NotFound);
                SendHttpResponse(ctx, req, res);
                return;
            }

            // Handshake
            var wsFactory = new WebSocketServerHandshakerFactory(
                GetWebSocketLocation(req), null, true, 5 * 1024 * 1024);
            this.handshaker = wsFactory.NewHandshaker(req);
            if (this.handshaker == null)
            {
                WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
            }
            else
            {
                this.handshaker.HandshakeAsync(ctx.Channel, req);
            }
        }

        static readonly byte[] StaticPlaintext = Encoding.UTF8.GetBytes("Hello, World!");
        static readonly int StaticPlaintextLen = StaticPlaintext.Length;
        static readonly IByteBuffer PlaintextContentBuffer = Unpooled.UnreleasableBuffer(Unpooled.DirectBuffer().WriteBytes(StaticPlaintext));
        static readonly AsciiString PlaintextClheaderValue = AsciiString.Cached($"{StaticPlaintextLen}");
        static readonly AsciiString JsonClheaderValue = AsciiString.Cached($"{JsonLen()}");
        static int JsonLen() => Encoding.UTF8.GetBytes(NewMessage().ToJsonFormat()).Length;
        static MessageBody NewMessage() => new MessageBody("Hello, World!");

        static readonly AsciiString TypePlain = AsciiString.Cached("text/plain");
        static readonly AsciiString TypeJson = AsciiString.Cached("application/json");
        static readonly AsciiString ServerName = AsciiString.Cached("Netty");
        static readonly AsciiString ContentTypeEntity = HttpHeaderNames.ContentType;
        static readonly AsciiString DateEntity = HttpHeaderNames.Date;
        static readonly AsciiString ContentLengthEntity = HttpHeaderNames.ContentLength;
        static readonly AsciiString ServerEntity = HttpHeaderNames.Server;

        static readonly ThreadLocalCache Cache = new ThreadLocalCache();

        sealed class ThreadLocalCache : FastThreadLocal<AsciiString>
        {
            protected override AsciiString GetInitialValue()
            {
                DateTime dateTime = DateTime.UtcNow;
                return AsciiString.Cached($"{dateTime.DayOfWeek}, {dateTime:dd MMM yyyy HH:mm:ss z}");
            }
        }
        volatile ICharSequence date = Cache.Value;

        void WriteResponse(IChannelHandlerContext ctx, IByteBuffer buf, ICharSequence contentType, ICharSequence contentLength)
        {
            // Build the response object.
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, buf, false);
            HttpHeaders headers = response.Headers;
            headers.Set(ContentTypeEntity, contentType);
            headers.Set(ServerEntity, ServerName);
            headers.Set(DateEntity, this.date);
            headers.Set(ContentLengthEntity, contentLength);

            // Close the non-keep-alive connection after the write operation is done.
            ctx.WriteAsync(response);
        }

        void HandleWebSocketFrame(IChannelHandlerContext ctx, WebSocketFrame frame)
        {
            // Check for closing frame
            if (frame is CloseWebSocketFrame)
            {
                this.handshaker.CloseAsync(ctx.Channel, (CloseWebSocketFrame)frame.Retain());
                return;
            }

            if (frame is PingWebSocketFrame)
            {
                ctx.WriteAsync(new PongWebSocketFrame((IByteBuffer)frame.Content.Retain()));
                return;
            }


            if (frame is TextWebSocketFrame)
            {
                // Echo the frame
                ctx.WriteAsync(frame.Retain());
                return;
            }

            if (frame is BinaryWebSocketFrame)
            {
                // Echo the frame
                ctx.WriteAsync(frame.Retain());
            }
        }

        static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        {
            // Generate an error page if response getStatus code is not OK (200).
            if (res.Status.Code != 200)
            {
                IByteBuffer buf = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(res.Status.ToString()));
                res.Content.WriteBytes(buf);
                buf.Release();
                HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
            }

            // Send the response and close the connection if necessary.
            Task task = ctx.Channel.WriteAndFlushAsync(res);
            if (!HttpUtil.IsKeepAlive(req) || res.Status.Code != 200)
            {
                task.ContinueWith((t, c) => ((IChannelHandlerContext)c).CloseAsync(),
                    ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.WriteLine($"{nameof(WebSocketServerHandler)} {0}", e);
            ctx.CloseAsync();
        }

        static string GetWebSocketLocation(IFullHttpRequest req)
        {
            bool result = req.Headers.TryGet(HttpHeaderNames.Host, out ICharSequence value);
            Debug.Assert(result, "Host header does not exist.");
            string location = value.ToString() + WebsocketPath;

            if (ServerSettings.IsSsl)
            {
                return "wss://" + location;
            }
            else
            {
                return "ws://" + location;
            }
        }
    }
}
