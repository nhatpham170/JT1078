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
using JT1078NetCore.Socket;
using System.IO;


namespace JT1078NetCore.Http
{
    public class HttpServerHandler : SimpleChannelInboundHandler<IFullHttpRequest>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            string[] arr = req.Uri.Split('?');
            string path = arr[0];

            if (path.StartsWith("/live/"))
            {
                // is websocket 
                string token = path.Substring(path.Length - 32);
                SessionProxy sessionProxy;
                if (Global.SESSIONS_PROXY.TryGetValue(token, out sessionProxy))
                {
                    var response = new DefaultHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK);
                    response.Headers.Set(HttpHeaderNames.ContentType, "video/x-flv");
                    response.Headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.KeepAlive);
                    response.Headers.Set(HttpHeaderNames.TransferEncoding, HttpHeaderValues.Chunked);
                    response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
                    response.Headers.Set(HttpHeaderNames.CacheControl, "no-cache");                 
                    ctx.WriteAndFlushAsync(response);                
                    sessionProxy.SetSession(ctx);
                    Global.SESSIONS_MAIN[sessionProxy.Key].AddSubscribe(sessionProxy);
                }
                else
                {
                    // token invalid
                    ctx.Channel.CloseAsync();
                }                
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
                string token;
                if (Global.CHANNEL_PROXY.TryGetValue(channelId, out token))
                {
                    SessionProxy sessionProxy;
                    if (Global.SESSIONS_PROXY.TryGetValue(token, out sessionProxy))
                    {
                        sessionProxy.Close();
                    }
                }
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
        
        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.WriteLine($"{nameof(WebSocketServerHandler)} {0}", e);
            ctx.CloseAsync();
        }
   
    }
}
