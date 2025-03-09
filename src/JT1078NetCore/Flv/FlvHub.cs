using JT1078NetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace JT1078NetCore.Flv
{  
    public class FlvHub : Hub
    {
        private readonly ILogger logger;
        private readonly WebSocketSession wsSession;

        public FlvHub(
            WebSocketSession wsSession,
            ILoggerFactory loggerFactory)
        {
            this.wsSession = wsSession;
            logger = loggerFactory.CreateLogger<FlvHub>();
        }

        public override Task OnConnectedAsync()
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"链接上:{Context.ConnectionId}");
            }
            wsSession.TryAdd(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"断开链接:{Context.ConnectionId}");
            }
            wsSession.TryRemove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
