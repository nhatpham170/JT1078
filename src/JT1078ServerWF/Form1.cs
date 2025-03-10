using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using JT1078NetCore.Http;
using DotNetty.Codecs;
using JT1078NetCore.Socket;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebSocketSharp.Server;
using static JT1078NetCore.Http.WsService;
using static System.Net.WebRequestMethods;
using System;

namespace JT1078ServerWF
{
    public partial class Form1 : Form
    {
        private static ServerBootstrap bootstrap;
        private static IChannel bootstrapChannel;
        public static ConcurrentDictionary<string, IChannel> Channels = new ConcurrentDictionary<string, IChannel>();
        public static ConcurrentDictionary<string, SocketSession> Sessions = new ConcurrentDictionary<string, SocketSession>();
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
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
            new WsService().Init();
            bootstrapChannel = await bootstrap.BindAsync(2202);
        }

        private async void InitHTTP()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddControllers();
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

        }

        private void btnHttpInit_Click(object sender, EventArgs e)
        {
            HttpServer httpServer = new HttpServer(5002);
            httpServer.AddWebSocketService<WsSession>("/live2");
            httpServer.AddWebSocketService<WsSession>("/ChatWithNyan");
            httpServer.Start();
        }
    }
}
