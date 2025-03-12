using DotNetty.Transport.Channels;
using JT1078.Flv;
using JT1078.Protocol;
using JT1078NetCore.Common;
using JT1078NetCore.Utils;
using Newtonsoft.Json.Linq;
using System.Threading.Channels;
using WebSocketSharp.Server;

namespace JT1078NetCore.Socket
{
    public class SocketSession
    {
        const string TYPE_LOG = "session";
        public string Protocol { get; set; }
        public int FormatMedia { get; set; }
        public string PlayType { get; set; } // live | playback
        public string Imei { get; set; }
        public int Chl { get; set; }
        public int StreamType { get; set; }
        public bool IsConnected { get; set; }
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
        public object LastIFrame { get; set; }
        public bool isStop = false;
        public long _initAt { get; set; }

        public string Key
        {
            get { return string.IsNullOrEmpty(_key) ? SetKey() : _key; }
        }
        public void Start(IChannelHandlerContext channel)
        {
            IsConnected = true;
            Channel = channel;
            ChannelId = channel.Channel.Id.ToString();
            Log.WriteFeatureLog($"[START]: session: {Key}", TYPE_LOG);
            Update();
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
        public void InitSession()
        {
            _initAt = DateUtil.Unix;
            Log.WriteFeatureLog($"[INIT]: session: {Key}", TYPE_LOG);
        }

        public static string NewToken()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToLower();
        }

        public void RemoveSubscribe(string token)
        {
            if (Subscribers.ContainsKey(token))
            {
                Log.WriteFeatureLog($"[REMOVE-TOKEN]: session: {Key}, token: {token}", TYPE_LOG);
                Subscribers.Remove(token);
                //Update();
            }
        }

        public void AddSubscribe(SessionProxy proxy)
        {
            Log.WriteFeatureLog($"[ADD-TOKEN]: token: {proxy.Token}", TYPE_LOG);
            if (LastIFrame != null)
            {
                byte[] frame = new FlvEncoder().EncoderVideoTag(LastIFrame as JT1078Package, true);
                proxy.SendMsg(frame);
            }
            Subscribers[proxy.Token] = proxy;
            //Update();
        }

        private void Update()
        {
            Global.SESSIONS_MAIN[Key] = this;
        }
        public long _startTimeout = 0;
        public void CheckTimeout()
        {
            if (IsConnected)
            {
                int count = Subscribers.Count;
                if (count == 0)
                {
                    if (_startTimeout == 0)
                    {
                        _startTimeout = DateUtil.Unix;
                    }
                    if (DateUtil.Unix - _startTimeout > 15)
                    {
                        // timeout
                        Log.WriteFeatureLog($"[TIMEOUT]: session: {Key}, Not player", TYPE_LOG);
                        Stop();                        
                    }
                }
                else { _startTimeout = 0; }
            }
            else
            {
                if (DateUtil.Unix - _initAt > 15)
                {
                    Log.WriteFeatureLog($"[TIMEOUT]: session: {Key}, Device not connect", TYPE_LOG);
                }
            }
        }
        public void Broadcast(byte[] data)
        {
            try
            {                
                // push data
                foreach (var proxy in Subscribers.Values)
                {
                    proxy.SendMsg(data);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        public void Destroy()
        {
            try
            {
                Global.SESSIONS_MAIN.TryRemove(Key, out _);
                foreach (SessionProxy item in Subscribers.Values)
                {
                    item.Destroy();
                    Log.WriteFeatureLog($"[DESTROY]: session: {Key}, token: {item.Token}", TYPE_LOG);
                }
                Log.WriteFeatureLog($"[DESTROY]: session: {Key}", TYPE_LOG);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }

        }

        public void Stop()
        {
            try
            {
                if (!isStop)
                {
                    isStop = true;
                    Global.SESSIONS_CHANNEL.TryGetValue(Key, out _);
                    IChannel channel;
                    if (Global.DictChannels.TryGetValue(ChannelId, out channel))
                    {
                        channel.CloseAsync();
                    }
                    Log.WriteFeatureLog($"[STOP]: session: {Key}", TYPE_LOG);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
    }
}
