using JT1078NetCore.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace JT1078NetCore.Cache
{
    public class RedisService
    {
        private static bool _isReady = false;
        private static ConnectionMultiplexer _conn;
        private static IDatabase _db;        
        private static volatile RedisService _intance;
        private const int TIME_EXPIRE = 1296000; // 15 Day
        protected RedisService()
        {
            _intance = this;
        }
        public static RedisService Instance
        {
            get
            {
                if (_intance == null)
                {
                    lock (typeof(RedisService))
                    {
                        if (_intance == null)
                        {
                            _intance = new RedisService(Global.RedisConnStr);                            
                        }
                    }
                }
                return _intance;
            }
        }    

        public RedisService(string connection)
        {
            _conn = ConnectionMultiplexer.Connect(connection);
            _db = _conn.GetDatabase();
            _isReady = true;
        }
        public bool Set(string key, object value)
        {
            if (_isReady)
            {
                return _db.StringSet(key, JsonConvert.SerializeObject(value));
            } 
            return false;
            
        }
        public string Get(string key)
        {
            if (_isReady)
            {
                string value = _db.StringGet(key).ToString();
                return string.IsNullOrEmpty(value) ? "" : value;
            }

            return "";
        }
    }
}
