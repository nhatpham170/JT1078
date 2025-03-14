using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using JT1078NetCore.Socket;
using JT1078NetCore.Utils;
using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace JT1078NetCore.Http
{
    public class HttpApiLive
    {
        public HttpApiLive() { }
        public HttpApiLive(string url) { }
        public const string TYPE = MediaDefine.PlayType.Live;
        public static string Protocol()
        {
            return "http://localhost:8080/live";
        }
        public static string ProtocolWs()
        {
            return "ws://localhost:8080/live";
        }
        public struct LiveResponse
        {
            public string token { get; set; }
            public int status { get; set; }
            public string link { get; set; }            
            //public string linkWs { get; set; }
        }

        public static void Process(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            try
            {
                // parser
                string[] arr = req.Uri.Split('?');
                var queryParmas = HttpUtility.ParseQueryString(arr.Length > 1 ? arr[1] : "");
                string imei = queryParmas.Get("imei").ToString().ToLower();
                string ch = queryParmas.Get("ch").ToString().ToLower();
                string streamType = queryParmas.Get("streamType").ToString().ToLower();
                string path = arr[0];                
                LiveResponse response = new LiveResponse();
                response.token = SocketSession.NewToken();
                response.status = 1;
                response.link = $"{ProtocolWs()}/{imei}_{ch}_{streamType}_{response.token}";
                SocketSession session = new SocketSession();
                session.PlayType = TYPE;
                session.Imei = imei;
                session.Chl = int.Parse(ch);
                session.StreamType = int.Parse(streamType);
                string key = session.SetKey();
                // add origin
                SocketSession sessionOrigin;
                if (!Global.SESSIONS_MAIN.TryGetValue(session.Key, out sessionOrigin))
                {
                    sessionOrigin = session;
                }                
                Global.SESSIONS_MAIN[sessionOrigin.Key] = sessionOrigin;
                // add proxy
                SessionProxy sessionProxy = new SessionProxy(response.token);
                sessionProxy.Key = key;
                Global.SESSIONS_PROXY[response.token] = sessionProxy;

                // add proxy
                Reponse(ctx, req, response);
            }
            catch (Exception ex)
            {
                LiveResponse response = new LiveResponse();                
                response.token = string.Empty;
                response.status = 0;
                response.link =string.Empty;
                Reponse(ctx, req, response);
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        private static void Reponse(IChannelHandlerContext ctx, IFullHttpRequest req, LiveResponse dataResponse)
        {
            try
            {
                IByteBuffer content = Unpooled.WrappedBuffer(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dataResponse)));
                var res = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, content);
                res.Headers.Set(HttpHeaderNames.ContentType, "application/json; charset=UTF-8");
                
                res.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, "true");
                res.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, "X-Requested-With, Content-Type, Authorization, Gps.App.Version, Origin, Accept, Access-Control-Request-Method, Access-Control-Request-Headers");
                res.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, "POST, GET, PUT, DELETE");
                res.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");                

                HttpUtil.SetContentLength(res, content.ReadableBytes);
                HttpUtils.SendHttpResponse(ctx, req, res);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
    }
}