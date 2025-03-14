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
    public class HttpFlvPusher: SessionProxy
    {
        private IChannelHandlerContext _channel { get; set; }
        public override void SetSession(IChannelHandlerContext channel)
        {
            _channel = channel;
        }
        public override void SendMsg(byte[] data)
        {
            _channel.WriteAndFlushAsync(data).Wait();
        }

        public override void Close()
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
    }
}