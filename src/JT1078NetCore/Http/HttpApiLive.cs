using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using JT1078NetCore.Socket;
using JT1078NetCore.Utils;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using WebSocketSharp.Net;
using static JT1078NetCore.Common.MediaDefine;

namespace JT1078NetCore.Http
{
    public class HttpApiLive
    {
        public HttpApiLive() { }
        public HttpApiLive(string url) { }
        //public const string TYPE = MediaDefine.PlayType.Live;
        public static string ProtocolWs()
        {
            if (Global.IsSsl)
            {
                return $"wss://{Global.WsHost}/live";
            }
            else
            {
                return $"ws://{Global.WsHost}:{Global.WsPort}/live";
            }
        }

        public static string Protocol(MediaDefine.MediaType mediaType, string playType = MediaDefine.PlayType.Live)
        {
            switch (mediaType)
            {

                case MediaDefine.MediaType.HttpFlv:
                    if (Global.IsSsl)
                        return $"https://{Global.APIHost}/{playType}";
                    else return $"http://{Global.APIHost}:{Global.HttpFlvPort}/{playType}";
                case MediaDefine.MediaType.WebSocketFlv:
                default:
                    if (Global.IsSsl)
                        return $"wss://{Global.WsHost}/{playType}";
                    else
                        return $"ws://{Global.WsHost}:{Global.WsPort}/{playType}";
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
                string playType = MediaDefine.PlayType.Live;
                if (req.Method.Name == "OPTIONS")
                {
                    LiveResponse resp = new LiveResponse();
                    resp.token = SocketSession.NewToken();
                    resp.status = 1;
                    resp.link = "";
                    Reponse(ctx, req, resp);
                    return;
                }
                string[] arr = req.Uri.Split('?');
                NameValueCollection queryParmas = HttpUtility.ParseQueryString(arr.Length > 1 ? arr[1] : "");
                string[] keys = queryParmas.AllKeys;
                RequestLiveModel liveModel = new RequestLiveModel();
                liveModel.Imei = keys.Contains("imei") ? queryParmas.Get("imei") : liveModel.Imei;
                liveModel.Channel = keys.Contains("ch") ? queryParmas.Get("ch") : liveModel.Channel;
                string qStreamType = keys.Contains("streamType") ? queryParmas.Get("streamType") : string.Empty;
                string mediaType = keys.Contains("mediaType") ? queryParmas.Get("mediaType") : string.Empty;
                if (qStreamType == "1")
                {
                    liveModel.StreamType = MediaDefine.StreamType.Main;
                }
                if (!string.IsNullOrEmpty(mediaType))
                {
                    switch (byte.Parse(mediaType))
                    {
                        case ((byte)MediaDefine.MediaType.WebSocketFlv):
                            liveModel.MediaType = MediaDefine.MediaType.WebSocketFlv;
                            break;
                        case ((byte)MediaDefine.MediaType.HttpFlv):
                        default:
                            liveModel.MediaType = MediaDefine.MediaType.HttpFlv;
                            break;
                    }
                }
                // fixed http-flv
                liveModel.MediaType = MediaDefine.MediaType.HttpFlv;
               
                switch (liveModel.MediaType)
                {
                    case MediaDefine.MediaType.HttpFlv:
                    case MediaDefine.MediaType.WebSocketFlv:
                    default:
                        liveModel.Extention = "flv";
                        break;
                }
                string path = arr[0];
                LiveResponse response = new LiveResponse();
                response.token = SocketSession.NewToken();
                response.status = 1;
                response.link = $"{Protocol(liveModel.MediaType)}/{liveModel.Imei}_{liveModel.Channel}_{(int)liveModel.StreamType}_{response.token}_{(int)liveModel.MediaType}.{liveModel.Extention}";               
                string pathProxy = $"/{playType}/{liveModel.Imei}_{liveModel.Channel}_{(int)liveModel.StreamType}_{response.token}_{(int)liveModel.MediaType}.{liveModel.Extention}";

                SocketSession session = new SocketSession();
                session.PlayType = playType;
                session.Imei = liveModel.Imei;
                session.Chl = int.Parse(liveModel.Channel);
                session.StreamType = liveModel.StreamType;
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
                SessionProxy sessionProxy = new SessionProxy(response.token);
                sessionProxy.Key = key;
                switch (liveModel.MediaType)
                {
                    case MediaDefine.MediaType.HttpFlv:
                        sessionProxy.MediaType = MediaDefine.MediaType.HttpFlv;
                        Global.SESSIONS_PROXY[response.token] = sessionProxy;
                        break;
                    default:
                    case MediaDefine.MediaType.WebSocketFlv:
                        sessionProxy.MediaType = MediaDefine.MediaType.WebSocketFlv;
                        Global.SESSIONS_PROXY[response.token] = sessionProxy;
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

        public static void ProcessPlayback(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            try
            {
                string playType = MediaDefine.PlayType.Playback;
                if (req.Method.Name == "OPTIONS")
                {
                    LiveResponse resp = new LiveResponse();
                    resp.token = SocketSession.NewToken();
                    resp.status = 1;
                    resp.link = "";
                    Reponse(ctx, req, resp);
                    return;
                }
                string[] arr = req.Uri.Split('?');
                NameValueCollection queryParmas = HttpUtility.ParseQueryString(arr.Length > 1 ? arr[1] : "");
                string[] keys = queryParmas.AllKeys;
                RequestLiveModel liveModel = new RequestLiveModel();
                liveModel.Imei = keys.Contains("imei") ? queryParmas.Get("imei") : liveModel.Imei;
                liveModel.Channel = keys.Contains("ch") ? queryParmas.Get("ch") : liveModel.Channel;
                string qStreamType = keys.Contains("streamType") ? queryParmas.Get("streamType") : string.Empty;
                string mediaType = keys.Contains("mediaType") ? queryParmas.Get("mediaType") : string.Empty;
                string startTime = keys.Contains("startTime") ? queryParmas.Get("startTime") : string.Empty;
                string endTime = keys.Contains("endTime") ? queryParmas.Get("endTime") : string.Empty;
                if (qStreamType == "1")
                {
                    liveModel.StreamType = MediaDefine.StreamType.Main;
                }
                if (!string.IsNullOrEmpty(mediaType))
                {
                    switch (byte.Parse(mediaType))
                    {
                        case ((byte)MediaDefine.MediaType.WebSocketFlv):
                            liveModel.MediaType = MediaDefine.MediaType.WebSocketFlv;
                            break;
                        case ((byte)MediaDefine.MediaType.HttpFlv):
                        default:
                            liveModel.MediaType = MediaDefine.MediaType.HttpFlv;
                            break;
                    }
                }
                // fixed http-flv
                liveModel.MediaType = MediaDefine.MediaType.HttpFlv;

                switch (liveModel.MediaType)
                {
                    case MediaDefine.MediaType.HttpFlv:
                    case MediaDefine.MediaType.WebSocketFlv:
                    default:
                        liveModel.Extention = "flv";
                        break;
                }
                string path = arr[0];
                LiveResponse response = new LiveResponse();
                response.token = SocketSession.NewToken();
                response.status = 1;
                response.link = $"{Protocol(liveModel.MediaType, PlayType.Playback)}/{liveModel.Imei}_{liveModel.Channel}_{(int)liveModel.StreamType}_{response.token}_{(int)liveModel.MediaType}.{liveModel.Extention}";
                string pathProxy = $"/{playType}/{liveModel.Imei}_{liveModel.Channel}_{(int)liveModel.StreamType}_{response.token}_{(int)liveModel.MediaType}.{liveModel.Extention}";

                SocketSession session = new SocketSession();
                session.PlayType = playType;
                session.Imei = liveModel.Imei;
                session.Chl = int.Parse(liveModel.Channel);
                session.StreamType = liveModel.StreamType;
                
                string key = session.SetKey( null, response.token);
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
                    response.isReady = false;
                }
                // add proxy
                SessionProxy sessionProxy = new SessionProxy(response.token);
                sessionProxy.Key = key;                
                switch (liveModel.MediaType)
                {
                    case MediaDefine.MediaType.HttpFlv:
                        sessionProxy.MediaType = MediaDefine.MediaType.HttpFlv;
                        Global.SESSIONS_PROXY[response.token] = sessionProxy;
                        break;
                    default:
                    case MediaDefine.MediaType.WebSocketFlv:
                        sessionProxy.MediaType = MediaDefine.MediaType.WebSocketFlv;
                        Global.SESSIONS_PROXY[response.token] = sessionProxy;
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
                if (Global.SESSIONS_PROXY.TryGetValue(token, out sessionProxy))
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