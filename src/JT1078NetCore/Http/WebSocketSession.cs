using System.Collections.Concurrent;

namespace JT1078NetCore.Http
{
    public class WebSocketSession
    {
        private ConcurrentDictionary<string, string> sessions;

        public WebSocketSession()
        {
            sessions = new ConcurrentDictionary<string, string>();
        }

        public void TryAdd(string connectionId)
        {
            sessions.TryAdd(connectionId, connectionId);
        }

        public int GetCount()
        {
            return sessions.Count;
        }

        public void TryRemove(string connectionId)
        {
            sessions.TryRemove(connectionId, out _);
        }

        public List<string> GetAll()
        {
            return sessions.Keys.ToList();
        }
    }
}
