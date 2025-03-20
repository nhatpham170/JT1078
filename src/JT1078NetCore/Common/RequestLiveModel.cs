using DotNetty.Transport.Channels;
using JT1078NetCore.Socket;
using System.Collections.Concurrent;
using System.Net;

namespace JT1078NetCore.Common
{
    public class RequestLiveModel
    {

        public string Imei { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Extention { get; set; } = "";
        public string PlayType { get; set; } = MediaDefine.PlayType.Live;
        public MediaDefine.MediaType MediaType { get; set; } = MediaDefine.MediaType.HttpFlv;
        public MediaDefine.StreamType StreamType { get; set; } = MediaDefine.StreamType.Sub;
    }
}
