using DotNetty.Transport.Channels;
using JT1078NetCore.Socket;
using System.Collections.Concurrent;
using System.Net;

namespace JT1078NetCore.Common
{
    public class RequestPlaybackModel : RequestLiveModel
    {

        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public RequestPlaybackModel()
        {
            PlayType = MediaDefine.PlayType.Playback;
            StartTime = string.Empty;
            EndTime = string.Empty;
        }
    }
}
