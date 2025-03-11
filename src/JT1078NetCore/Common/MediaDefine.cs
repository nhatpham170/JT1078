using DotNetty.Transport.Channels;
using JT1078NetCore.Socket;
using System.Collections.Concurrent;
using System.Net;

namespace JT1078NetCore.Common
{
    public class MediaDefine
    {        
        public class PlayType
        {
            public const string Live = "live";
            public const string Playback = "playback";             
        }
        public enum SessionStatus
        {
            Unknown = 0,
            Subscribe = 1,
            Destroy = 2,
        }
    }
}
