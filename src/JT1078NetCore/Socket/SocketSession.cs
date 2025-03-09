using DotNetty.Transport.Channels;

namespace JT1078NetCore.Socket
{
    public class SocketSession
    {
        public string Protocol { get; set; }
        public int FormatMedia { get; set; }
        public string Feature { get; set; } // live | playback
        public string Imei { get; set; }
        public int Chl { get; set; }
        public string ChannelId { get; set; }
        public string Reverse { get; set; } = string.Empty;
        public bool Valid { get; set; }
        private string _key = "";
        public bool HasFlvHeader { get; set; } = false;
        public List<string> Packages { get; set; } = new List<string>();


        public string Key
        {
            get { return _key; }
        }
        public string SetKey(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                _key = $"{Feature}_{Imei}_{Chl}";
            }
            else
            {
                _key = key;
            }
            return _key;
            
        }
    }
}
