using JT1078NetCore.Cache.Models;
using Newtonsoft.Json;

namespace JT1078NetCore.Cache
{
    public class DFSessions
    {
        private const string TBL_NAME = "tbl_sessions";
        private static volatile DFSessions _intance;
        private const int TIME_EXPIRE = 1296000; // 15 Day
        protected DFSessions()
        {
            _intance = this;
        }
        public static DFSessions Instance
        {
            get
            {
                if (_intance == null)
                {
                    lock (typeof(DFSessions))
                    {
                        if (_intance == null)
                        {
                            _intance = new DFSessions();
                        }
                    }
                }
                return _intance;
            }
        }
        public CacheSession Get(string imei)
        {
            CacheSession cacheSession;
            cacheSession = JsonConvert.DeserializeObject<CacheSession>(RedisService.Instance.Get(string.Format("{0}{1}:{2}", CacheHelper.CacheKeyPrefix, TBL_NAME, imei)));
            if (cacheSession != null 
                && cacheSession.TimestampStart > 0 
                && cacheSession.TimestampStart > cacheSession.TimestampEnd)
            {
                cacheSession = new CacheSession();
                cacheSession.Valid = false;
            }

            return cacheSession;
        }

        public bool CheckValid(string imei)
        {
            CacheSession cacheSession;
            cacheSession = JsonConvert.DeserializeObject<CacheSession>(RedisService.Instance.Get(string.Format("{0}{1}:{2}", CacheHelper.CacheKeyPrefix, TBL_NAME, imei)));
            if (cacheSession != null
                && cacheSession.TimestampStart > 0
                && cacheSession.Valid)
            {
                return true;
            }

            return false;
        }
    }
}
