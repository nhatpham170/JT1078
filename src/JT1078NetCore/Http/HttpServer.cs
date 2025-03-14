using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Transport.Libuv;
using DotNetty.Handlers.Tls;
using DotNetty.Codecs.Http;
using JT1078NetCore.Utils;

namespace JT1078NetCore.Http
{
    public class HttpServer
    {
        public HttpServer()
        {

        }
        public async Task Init(int port)
        {
           Log.WriteStatusLog(
                $"\n{RuntimeInformation.OSArchitecture} {RuntimeInformation.OSDescription}"
                + $"\n{RuntimeInformation.ProcessArchitecture} {RuntimeInformation.FrameworkDescription}"
                + $"\nProcessor Count : {Environment.ProcessorCount}\n");

            bool useLibuv = ServerSettings.UseLibuv;
            Log.WriteStatusLog("Transport type : " + (useLibuv ? "Libuv" : "Socket"));

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

             Log.WriteStatusLog($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
             Log.WriteStatusLog($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");
            Log.WriteStatusLog("\n");
            IEventLoopGroup bossGroup;
            IEventLoopGroup workGroup;
            //if (useLibuv)
            //{
            //    var dispatcher = new DispatcherEventLoopGroup();
            //    bossGroup = dispatcher;
            //    workGroup = new WorkerEventLoopGroup(dispatcher);
            //}
            //else
            //{
            //    bossGroup = new MultithreadEventLoopGroup(1);
            //    workGroup = new MultithreadEventLoopGroup();
            //}
            bossGroup = new MultithreadEventLoopGroup(1);
            workGroup = new MultithreadEventLoopGroup();
            X509Certificate2 tlsCertificate = null;
            //if (ServerSettings.IsSsl)
            //{
            //    tlsCertificate = new X509Certificate2(Path.Combine(CommonHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            //}
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workGroup);

                //if (useLibuv)
                //{
                //    bootstrap.Channel<TcpServerChannel>();
                //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                //        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                //    {
                //        bootstrap
                //            .Option(ChannelOption.SoReuseport, true)
                //            .ChildOption(ChannelOption.SoReuseaddr, true);
                //    }
                //}
                //else
                //{
                //    bootstrap.Channel<TcpServerSocketChannel>();
                //}

                bootstrap
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 8192 * 1024)
                    .Option(ChannelOption.SoReuseport, true)
                    .Option(ChannelOption.TcpNodelay, true)
                    .ChildOption(ChannelOption.SoReuseaddr, true)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {

                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        pipeline.AddLast(new HttpRequestDecoder());
                        pipeline.AddLast(new HttpResponseEncoder());
                        pipeline.AddLast(new HttpObjectAggregator(65536));
                        // HTTP compression
                        //pipeline.AddLast("compressor", new HttpContentCompressor());
                        pipeline.AddLast(new HttpServerHandler());
                    }));

                //int port = ServerSettings.Port;
                IChannel bootstrapChannel = await bootstrap.BindAsync(IPAddress.Any, port);

                Log.WriteStatusLog("Open your web browser and navigate to "
                    + $"{(ServerSettings.IsSsl ? "https" : "http")}"
                    + $"://127.0.0.1:{port}/");
                Log.WriteStatusLog("Listening on "
                    + $"{(ServerSettings.IsSsl ? "wss" : "ws")}"
                    + $"://127.0.0.1:{port}/websocket");

                //await bootstrapChannel.CloseAsync();
            }
            finally
            {
                //workGroup.ShutdownGracefullyAsync().Wait();
                //bossGroup.ShutdownGracefullyAsync().Wait();
            }
        }
        public  async Task Init2(int port)
        {
            Console.WriteLine(
                $"\n{RuntimeInformation.OSArchitecture} {RuntimeInformation.OSDescription}"
                + $"\n{RuntimeInformation.ProcessArchitecture} {RuntimeInformation.FrameworkDescription}"
                + $"\nProcessor Count : {Environment.ProcessorCount}\n");

            bool useLibuv = ServerSettings.UseLibuv;
            Console.WriteLine("Transport type : " + (useLibuv ? "Libuv" : "Socket"));

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

            Console.WriteLine($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
            Console.WriteLine($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");
            Console.WriteLine("\n");

            IEventLoopGroup bossGroup;
            IEventLoopGroup workGroup;
            if (useLibuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workGroup = new WorkerEventLoopGroup(dispatcher);
            }
            else
            {
                bossGroup = new MultithreadEventLoopGroup(1);
                workGroup = new MultithreadEventLoopGroup();
            }

            X509Certificate2 tlsCertificate = null;
            //if (ServerSettings.IsSsl)
            //{
            //    tlsCertificate = new X509Certificate2(Path.Combine(CommonHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            //}
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workGroup);

                if (useLibuv)
                {
                    bootstrap.Channel<TcpServerChannel>();
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        bootstrap
                            .Option(ChannelOption.SoReuseport, true)
                            .ChildOption(ChannelOption.SoReuseaddr, true);
                    }
                }
                else
                {
                    bootstrap.Channel<TcpServerSocketChannel>();
                }

                bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(65536));
                        pipeline.AddLast(new WebSocketServerHandler());
                    }));

                //int port = ServerSettings.Port;
                IChannel bootstrapChannel = await bootstrap.BindAsync(IPAddress.Any, port);

                Console.WriteLine("Open your web browser and navigate to "
                    + $"{(ServerSettings.IsSsl ? "https" : "http")}"
                    + $"://127.0.0.1:{port}/");
                Console.WriteLine("Listening on "
                    + $"{(ServerSettings.IsSsl ? "wss" : "ws")}"
                    + $"://127.0.0.1:{port}/websocket");

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                //workGroup.ShutdownGracefullyAsync().Wait();
                //bossGroup.ShutdownGracefullyAsync().Wait();
            }
        }

    }
}
