using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using JT1078NetCore.Utils;
using DotNetty.Codecs;
using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using JT1078NetCore.Http;
using JT1078NetCore.Common;

namespace JT1078NetCore.Socket
{
    public class SocketService : BackgroundService
    {
        private static ServerBootstrap bootstrap;
        private static IChannel bootstrapChannel;
        public static ConcurrentDictionary<string, IChannel> Channels = new ConcurrentDictionary<string, IChannel>();
        public static ConcurrentDictionary<string, SocketSession> Sessions = new ConcurrentDictionary<string, SocketSession>();      
        private readonly ILogger<SocketService> _logger;

        public SocketService(ILogger<SocketService> logger)
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var bossGroup = new MultithreadEventLoopGroup();
                var workerGroup = new MultithreadEventLoopGroup();
                bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup)
                    .Channel<TcpServerSocketChannel>()
                    //.Option(ChannelOption.SoBacklog, 8192)// 1024
                    .Option(ChannelOption.SoRcvbuf, 0x8340000) // 4,194,304 // 65536 // 32768 // 846750
                    .Option(ChannelOption.SoSndbuf, 0x8340000)
                    .Option(ChannelOption.TcpNodelay, true)
                      //.Option(ChannelOption.AutoRead, true)
                      //.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                      //.Option(ChannelOption.WriteBufferHighWaterMark, 1048576)              
                      //.Option(ChannelOption.SoSndbuf, 102400)
                      //.Option(ChannelOption.SoRcvbuf, 102400)                    
                      //.Handler(new LoggingHandler(LogLevel.INFO))
                      // howen
                      .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                      {
                          IChannelPipeline pipeline = channel.Pipeline;
                          channel.Pipeline.AddLast("encoder", new ByteEncoder());
                          channel.Pipeline.AddLast("hexDecoder", new HexDecoder());
                          channel.Pipeline.AddLast("dataFilter", new SocketProcess());
                          channel.Pipeline.AddLast(new StringDecoder(), new SocketHandler());
                      }));            
                new WsService().Init(Global.WsPort);
                return bootstrap.BindAsync(Global.TCPPort);

            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
            return null;
        }
    }
}
