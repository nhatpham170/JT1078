using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using JT1078NetCore.Utils;
using System.Threading.Channels;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace JT1078NetCore.Socket
{
    public class SessionProxy : WebSocketBehavior
    {
        public MediaDefine.MediaType MediaType = MediaDefine.MediaType.WebSocketFlv;
        public bool isValid = true;
        public string Token { get; set; }
        public string Key { get; set; }
        public long InitAt { get; set; }
        public long StartAt { get; set; }
        public long SentAt { get; set; }
        public long ReplyAt { get; set; }
        public long DestroyAt { get; set; }
        public string ChannelId { get; set; }
        public string Path { get; set; }
        public MediaDefine.SessionStatus Status { get; set; }
        private IChannelHandlerContext _channel { get; set; }
        public SessionProxy()
        {
            InitAt = DateUtil.Unix;
        }
        public SessionProxy(string token)
        {
            this.Token = token;
            InitAt = DateUtil.Unix;
        }        
        public virtual void SetSession(IChannelHandlerContext channel)
        {
            _channel = channel;
            this.Status = MediaDefine.SessionStatus.Subscribe;
        }
        //public virtual void SendMsg(byte[] data)
        //{
           
        //}
        //public void Subscribe(IChannelHandlerContext Channel)
        //{
        //    if (_channel != null)
        //    {
        //        _channel.CloseAsync().Wait();
        //    }
        //    _channel = Channel;
        //    StartAt = DateUtil.Unix;
        //    Status = MediaDefine.SessionStatus.Subscribe;
        //    SentAt = 0;
        //    ChannelId = Channel.Channel.Id.ToString();
        //    Global.CHANNEL_PROXY[ChannelId] = Token;
        //    Update();
        //}
        //public IChannelHandlerContext Channel()
        //{
        //    return this._channel;
        //}
        //public void Send(byte[] data)
        //{
        //    try
        //    {
        //        if (Status == MediaDefine.SessionStatus.Subscribe)
        //        {
        //            _channel.WriteAsync(data);
        //            SentAt = DateUtil.Unix;
        //        }
        //        Update();
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionHandler.ExceptionProcess(ex);
        //    }
        //}
        public void Receive(byte[] data)
        {
            ReplyAt = DateUtil.Unix;
            Update();
        }
        public virtual void SendMsg(byte[] data)
        {
            SentAt = DateUtil.Unix;
            if (MediaType == MediaDefine.MediaType.HttpFlv)
            {
                var buffer = Unpooled.WrappedBuffer(data);
                _channel.WriteAndFlushAsync(buffer);
                //_channel.Channel.Flush();
            }
            else {
                Send(data);
            }
        }


        protected override void OnOpen()
        {
            StartAt = DateUtil.Unix;
            string[] arr = Context.RequestUri.LocalPath.Split(new char [] { '.','_'});
            string path = arr[arr.Length - 1];
            Token = arr[3];
            //Key = $"{arr[0]}_{arr[1]}_{arr[2]}_{arr[3]}";
            Status = MediaDefine.SessionStatus.Subscribe;
            SentAt = 0;
            Path = Context.RequestUri.LocalPath;
            MediaType = MediaDefine.MediaType.WebSocketFlv;
            // check used
            SessionProxy old;
            if (Global.SESSIONS_PROXY.TryGetValue(Token, out old))
            {
                Key = old.Key;
                if (old.StartAt > 0)
                {
                    // block token                    
                    isValid = false;
                    Block(Path);
                    return;
                }
            }
            SocketSession socketSession;
            if (Global.SESSIONS_MAIN.TryGetValue(Key, out socketSession))
            {
                socketSession.AddSubscribe(this);
            }
        }
        private void Block(string path)
        {
            try
            {
                Log.WriteFeatureLog($"[BLOCK] token: {Token}, session: {Key}; token used ", "proxy");
                Global.WsServer.RemoveWebSocketService(path);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (isValid)
            {
                DestroyAt = DateUtil.Unix;
                Status = MediaDefine.SessionStatus.Destroy;
                SocketSession socketSession;
                if (Global.SESSIONS_MAIN.TryGetValue(Key, out socketSession))
                {
                    socketSession.RemoveSubscribe(Token);
                }
            }

        }
        protected override void OnMessage(MessageEventArgs e)
        {
            //var fmt = "{0}: {1}";
            //var msg = String.Format(fmt, _name, e.Data);

            //Sessions.Broadcast(msg);
        }
        public virtual void Close()
        {
            if (isValid)
            {
                DestroyAt = DateUtil.Unix;
                Status = MediaDefine.SessionStatus.Subscribe;
                if (_channel != null)
                {
                    _ = _channel.CloseAsync();
                }
                Global.SESSIONS_PROXY.TryRemove(Token, out _);
                Global.CHANNEL_PROXY.TryRemove(ChannelId, out _);
                // update session main
                SocketSession socketSession;
                if (Global.SESSIONS_MAIN.TryGetValue(Key, out socketSession))
                {
                    socketSession.RemoveSubscribe(Token);
                }
            }
        }
        private void Update()
        {
            Global.SESSIONS_PROXY[Token] = this;
        }

        public void Destroy()
        {
            try
            {
                Global.WsServer.RemoveWebSocketService(Path);
                Global.SESSIONS_PROXY.TryRemove(Token, out _);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
    }
}
