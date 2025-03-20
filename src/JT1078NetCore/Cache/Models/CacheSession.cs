namespace JT1078NetCore.Cache.Models
{
    public class CacheSession
    {
        public string IMEI { get; set; }
        public long TimestampStart { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public long TimeLive { get; set; }
        public long TimestampEnd { get; set; }
        public bool Valid { get; set; }
        public string Key { get; set; }
    }
}
