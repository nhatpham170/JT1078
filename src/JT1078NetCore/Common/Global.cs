using DotNetty.Transport.Channels;
using System.Collections.Concurrent;
using System.Net;

namespace JT1078NetCore.Common
{
    public class Global
    {
        public static string HostForward = "";
        public static int PortForward = 0;
        public static string HostForward2 = "";
        public static int PortForward2 = 0;
        public static string Host = "";
        public static int Port = 0;
        public static bool ConvertReply = false;
        public static string IPConvert = "";
        public static int PortConvert = 0;
        public static string Model = "";
        public static bool Init = false;        
        public static IPEndPoint UDP_EndPoint;
        public static uint UDP_SSRC = 0;
        public static ushort UDP_SQE = 0;
        public static uint UDP_TIME = 0;
        public static ulong LastTime = 0;
        
        public static string QueueLive;

        public static ConcurrentDictionary<string, IChannel> DictChannels = new ConcurrentDictionary<string, IChannel>();
        public static ConcurrentDictionary<string, string> DictBuffer = new ConcurrentDictionary<string, string>();        
        public static ConcurrentDictionary<string, string> DictStream = new ConcurrentDictionary<string, string>();        
        public static ConcurrentDictionary<string, IChannel> DictForwardChannels = new ConcurrentDictionary<string, IChannel>();
        public static ConcurrentDictionary<string, string> DictForwardMapperChannels = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, IChannel> DictForward2Channels = new ConcurrentDictionary<string, IChannel>();
        public static ConcurrentDictionary<string, string> DictForwardMapper2Channels = new ConcurrentDictionary<string, string>();        
    }
}
