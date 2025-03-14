using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using JT1078NetCore.Socket;
using JT1078NetCore.Utils;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using WebSocketSharp.Net;

namespace JT1078NetCore.Http
{
    public class HttpApiLive
    {
        public HttpApiLive() { }
        public HttpApiLive(string url) { }
        public const string TYPE = MediaDefine.PlayType.Live;   
        public static string ProtocolWs()
        {
            if (Global.IsSsl)
            {
                return $"wss://{Global.WsHost}/live";
            }else
            {
                return $"ws://{Global.WsHost}:{Global.WsPort}/live";
            }            
        }

        public static string Protocol(MediaDefine.MediaType mediaType)
        {
            switch (mediaType)
            {

                case MediaDefine.MediaType.HttpFlv:
                    if (Global.IsSsl)
                        return $"https://{Global.APIHost}/live";
                    else return $"http://{Global.APIHost}:{Global.HttpFlvPort}/live";                    
                case MediaDefine.MediaType.WebSocketFlv:
                default:
                    if (Global.IsSsl)
                        return $"wss://{Global.WsHost}/live";
                    else
                        return $"ws://{Global.WsHost}:{Global.WsPort}/live";                    
            }            
        }
        public struct LiveResponse
        {
            public string token { get; set; }
            public int status { get; set; }
            public string link { get; set; }
            public bool isReady { get; set; }
            //public string linkWs { get; set; }
        }
        public static void ProcessWS(HttpListenerRequest req, HttpListenerResponse res)
        {
            try
            {
                // parser
                string[] arr = req.RawUrl.Split('?');
                var queryParmas = HttpUtility.ParseQueryString(arr.Length > 1 ? arr[1] : "");
                string imei = queryParmas.Get("imei").ToString().ToLower();
                string ch = queryParmas.Get("ch").ToString().ToLower();
                string streamType = queryParmas.Get("streamType").ToString().ToLower();                
                string path = arr[0];
                LiveResponse response = new LiveResponse();
                response.token = SocketSession.NewToken();
                response.status = 1;
                
                //response.link = $"{Protocol(mediaType)}/{imei}_{ch}_{streamType}_{response.token}";
                string pathProxy = $"/{TYPE}/{imei}_{ch}_{streamType}_{response.token}";
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
                    // new channel
                    sessionOrigin = session;
                    sessionOrigin.InitSession();
                    Global.SESSIONS_MAIN[sessionOrigin.Key] = sessionOrigin;
                }
                if (sessionOrigin.IsConnected)
                {
                    response.isReady = true;
                }
                // add proxy
                //SessionProxy sessionProxy = new SessionProxy(response.token);
                //sessionProxy.Key = key;
                //Global.SESSIONS_PROXY_WS[response.token] = pathProxy;
                Global.WsServer.AddWebSocketService<SessionProxy>(pathProxy);
                // add proxy
                ResponseWs(req, res, response);
            }
            catch (Exception ex)
            {
                LiveResponse response = new LiveResponse();
                response.token = string.Empty;
                response.status = 0;
                response.link = string.Empty;
                ResponseWs(req, res, response);
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        private static void ResponseWs(HttpListenerRequest req, HttpListenerResponse res, LiveResponse dataResponse)
        {
            byte[] content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataResponse));
            res.AppendHeader("content-type", "application/json; charset=UTF-8");              
            //res.Headers.Add("access-control-allow-credentials", "true");
            //res.Headers.Add("access-control-allow-headers", "X-Requested-With, Content-Type, Authorization, Gps.App.Version, Origin, Accept, Access-Control-Request-Method, Access-Control-Request-Headers");
            //res.Headers.Add("access-control-allow-methods", "POST, GET, PUT, DELETE");
            //res.Headers.Add("access-control-allow-origin", "*");
            res.ContentLength64 = content.Length;
            res.Close(content, true);

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
                string mediaTypeStr = (queryParmas.Get("mediaType") != null
                        ? queryParmas.Get("mediaType").ToString().ToLower() : "0");
                MediaDefine.MediaType mediaType = (MediaDefine.MediaType)int.Parse(mediaTypeStr);
                string path = arr[0];
                LiveResponse response = new LiveResponse();
                response.token = SocketSession.NewToken();
                response.status = 1;
                response.link = $"{Protocol(mediaType)}/{imei}_{ch}_{streamType}_{response.token}.flv";
                string pathProxy = $"/{TYPE}/{imei}_{ch}_{streamType}_{response.token}";
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
                    // new channel
                    sessionOrigin = session;
                    sessionOrigin.InitSession();
                    Global.SESSIONS_MAIN[sessionOrigin.Key] = sessionOrigin;
                }
                if (sessionOrigin.IsConnected)
                {
                    response.isReady = true;
                }
                // add proxy
                switch (mediaType)
                {
                    
                    case MediaDefine.MediaType.HttpFlv:
                        SessionProxy sessionProxy = new SessionProxy(response.token);
                        sessionProxy.Key = key;
                        sessionProxy.MediaType = MediaDefine.MediaType.HttpFlv;
                        Global.SESSIONS_PROXY[response.token] = sessionProxy;
                        break;
                    default:
                    case MediaDefine.MediaType.WebSocketFlv:
                        Global.WsServer.AddWebSocketService<SessionProxy>(pathProxy);
                        break;
                }                                
                // add proxy
                Reponse(ctx, req, response);
            }
            catch (Exception ex)
            {
                LiveResponse response = new LiveResponse();
                response.token = string.Empty;
                response.status = 0;
                response.link = string.Empty;
                Reponse(ctx, req, response);
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        public static void Close(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            try
            {
                // parser
                string[] arr = req.Uri.Split('?');
                string path = arr[0];                
                var queryParmas = HttpUtility.ParseQueryString(arr.Length > 1 ? arr[1] : "");
                string token = queryParmas.Get("token").ToString().ToLower();
                LiveResponse response = new LiveResponse();
                response.token = token;
                response.status = 1;
                response.link = string.Empty;
                SessionProxy sessionProxy;
                if(Global.SESSIONS_PROXY.TryGetValue(token, out sessionProxy))
                {
                    sessionProxy.Destroy();
                }

                Reponse(ctx, req, response);
            }
            catch (Exception ex)
            {
                LiveResponse response = new LiveResponse();
                response.token = string.Empty;
                response.status = 0;
                response.link = string.Empty;
                Reponse(ctx, req, response);
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        private static void Reponse(IChannelHandlerContext ctx, IFullHttpRequest req, LiveResponse dataResponse)
        {
            try
            {
                IByteBuffer content = Unpooled.WrappedBuffer(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dataResponse)));
                var res = new DefaultFullHttpResponse(DotNetty.Codecs.Http.HttpVersion.Http11, HttpResponseStatus.OK, content);
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