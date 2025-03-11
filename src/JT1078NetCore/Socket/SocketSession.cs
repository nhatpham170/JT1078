using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using WebSocketSharp.Server;

namespace JT1078NetCore.Socket
{
    public class SocketSession
    {
        public string Protocol { get; set; }
        public int FormatMedia { get; set; }
        public string PlayType { get; set; } // live | playback
        public string Imei { get; set; }
        public int Chl { get; set; }
        public int StreamType { get; set; }
        bool IsConnected { get; set; }
        public string ChannelId { get; set; }
        public IChannelHandlerContext Channel { get; set; }
        public string Reverse { get; set; } = string.Empty;
        public bool Valid { get; set; }
        private string _key = "";
        public bool HasFlvHeader { get; set; } = false;
        public List<string> Packages { get; set; } = new List<string>();
        public Dictionary<string, SessionProxy> Subscribers = new Dictionary<string, SessionProxy>();
        public byte[] FlvHeader { get; set; }
        public byte[] LastFrame { get; set; }

        public string Key
        {
            get { return string.IsNullOrEmpty(_key) ? SetKey() : _key; }
        }
        public void Start(IChannelHandlerContext channel)
        {
            IsConnected = true;
            Channel = channel;
            ChannelId = channel.Channel.Id.ToString();
        }
        public string SetKey(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                _key = $"{PlayType}_{Imei}_{Chl}_{StreamType}";
            }
            else
            {
                _key = key;
            }
            return _key;
            
        }

        public static string NewToken()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToLower();
        }

        public void RemoveSubscribe(string token)
        {
            if (Subscribers.ContainsKey(token))
            {
                Subscribers.Remove(token);
                Update();
            }
        }

        public void AddSubscribe(SessionProxy proxy)
        {            
            Subscribers[proxy.Token] =  proxy;
            Update();
        }

        private void Update()
        {
            Global.SESSIONS_MAIN[Key] = this;
        }

        public void Broadcast(byte[] data)
        {
            foreach (var proxy in Subscribers.Values)
            {
                if(proxy.SentAt == 0)
                {
                    proxy.SendMsg(FlvHeader);
                    //Send(FlvHeader);
                    //proxy.Send(FlvHeader);
                }
                //proxy.Send(data);
                proxy.SendMsg(data);
            }
        }
    }
}
