using DotNetty.Transport.Channels;
using JT1078NetCore.Socket;
using System.Collections.Concurrent;
using System.Net;

namespace JT1078NetCore.Common
{
    public class DateUtil
    {        
        public static long Unix
        {
            get {
                var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
                return (long)timeSpan.TotalSeconds;
            }
            
        }

        public static long Tick
        {
            get {
                var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
                return (long)timeSpan.Ticks;
            }            
        }
    }
}
