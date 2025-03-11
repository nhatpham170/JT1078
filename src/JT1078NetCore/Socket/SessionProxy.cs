using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using JT1078NetCore.Utils;
using System.Threading.Channels;

namespace JT1078NetCore.Socket
{
    public class SessionProxy
    {
        public string Token { get; set; }
        public string Key { get; set; }
        public long InitAt { get; set; }
        public long StartAt { get; set; }
        public long SentAt { get; set; }
        public long ReplyAt { get; set; }
        public long DestroyAt { get; set; }
        public string ChannelId { get; set; }
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
        public void Subscribe(IChannelHandlerContext Channel)
        {
            if (_channel != null)
            {
                _channel.CloseAsync().Wait();
            }
            _channel = Channel;
            StartAt = DateUtil.Unix;
            Status = MediaDefine.SessionStatus.Subscribe;
            SentAt = 0;
            ChannelId = Channel.Channel.Id.ToString();
            Global.CHANNEL_PROXY[ChannelId] = Token;
            Update();
        }
        public IChannelHandlerContext Channel()
        {
            return this._channel;
        }
        public void Send(byte[] data)
        {
            try
            {
                if (Status == MediaDefine.SessionStatus.Subscribe)
                {
                    _channel.WriteAsync(data);
                    SentAt = DateUtil.Unix;
                }
                Update();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        public void Receive(byte[] data)
        {
            ReplyAt = DateUtil.Unix;
            Update();
        }
        public void Close()
        {
            DestroyAt = DateUtil.Unix;
            Status = MediaDefine.SessionStatus.Subscribe;
            if (_channel == null)
            {
                _ = _channel.CloseAsync();
            }
            Global.SESSIONS_PROXY.TryRemove(Token, out _);
            Global.CHANNEL_PROXY.TryRemove(ChannelId, out _);
            // update session main
            SocketSession socketSession;
            if(Global.SESSIONS_MAIN.TryGetValue(Key, out socketSession))
            {
                socketSession.RemoveSubscribe(Token);
            }
        }
        private void Update()
        {
            Global.SESSIONS_PROXY[Token] = this;
        }
    }
}
