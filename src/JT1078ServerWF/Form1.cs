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
using System.Threading.Tasks;
using JT1078NetCore.Utils;
using Microsoft.AspNetCore.Http;
using System.Web;
using JT1078NetCore.Common;
using Newtonsoft.Json;
using Microsoft.VisualBasic;
using System.Text;

namespace JT1078ServerWF
{
    public partial class Form1 : Form
    {
        private bool status = false;
        private static ServerBootstrap bootstrap;
        private static IChannel bootstrapChannel;
        public static ConcurrentDictionary<string, IChannel> Channels = new ConcurrentDictionary<string, IChannel>();
        public static ConcurrentDictionary<string, SocketSession> Sessions = new ConcurrentDictionary<string, SocketSession>();
        public Form1()
        {
            InitializeComponent();
        }

        private void LoadConfig()
        {
            JT1078NetCore.Common.Global.TCPPort = int.Parse(txtTCPPort.Text);
            JT1078NetCore.Common.Global.APIHost = txtHostAPI.Text;
            JT1078NetCore.Common.Global.APIPort = int.Parse(txtPortAPI.Text);

            JT1078NetCore.Common.Global.WsPort = int.Parse(txtPortWs.Text);
            JT1078NetCore.Common.Global.WsHost = txtHostAPI.Text;

            JT1078NetCore.Common.Global.LogPath = txtLogPath.Text;
            Log.LogPathStr = JT1078NetCore.Common.Global.LogPath;
        }
        const string LOG_MONITOR = "monitor";
        private void Monitor()
        {
            try
            {
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 5000;
                timer.Tick += EventMonitor;
                timer.Start();
                Log.WriteFeatureLog("INIT", LOG_MONITOR);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        struct MonitorObj
        {
            public int Connection;
            public int Session;
            public int Proxy;
        }
        private void EventMonitor(object? sender, EventArgs e)
        {
            try
            {                
                MonitorObj obj = new MonitorObj();
                obj.Connection = Global.DictChannels.Count;
                obj.Session = Global.SESSIONS_MAIN.Count;
                obj.Proxy = Global.SESSIONS_PROXY.Count;
                Log.WriteFeatureLog($"[REPORT] sum: {JsonConvert.SerializeObject(obj)}", LOG_MONITOR);
                // check timeout                
                foreach (var item in Global.SESSIONS_MAIN.Values)
                {
                    try
                    {
                        item.CheckTimeout();
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.ExceptionProcess(ex);
                    }
                }             
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                LoadConfig();
                //Log.WriteDeviceLog("213213", "demo");
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
                bootstrapChannel = await bootstrap.BindAsync(Global.TCPPort);
                JT1078NetCore.Http.WebSocketServer webSocketServer = new JT1078NetCore.Http.WebSocketServer();
                await webSocketServer.Init(Global.APIPort);
                this.status = true;
                btnStart.BackColor = Color.GreenYellow;
                Monitor();
                Log.WriteStatusLog("Start service");
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
           
        }
        private async Task InitHTTP()
        {

        }

        private void btnHttpInit_Click(object sender, EventArgs e)
        {
            _ = InitHTTP();
            HttpServer httpServer = new HttpServer(2202);
            httpServer.OnGet += (sender, e) =>
            {
                var req = e.Request;
                var res = e.Response;
                var path = req.RawUrl;
                try
                {
                    if (path.StartsWith("/api/live"))
                    {
                        // add proxy
                        //Uri myUri = new Uri("http://www.example.com?param1=good&param2=bad");
                        //var paramQuery = HttpUtility.ParseQueryString(path);
                        //string app = paramQuery.Get("app").ToString();
                        //string imei = paramQuery.Get("imei").ToString();
                        //string ch = paramQuery.Get("ch").ToString();
                        //string token = Guid.NewGuid().ToString();                        
                        //byte[] contents  = Encoding.UTF8.GetBytes("xin chào");
                        //res.ContentLength64 = contents.LongLength;

                        //res.Close(contents, true);
                        HttpApiLive.ProcessWS(req, res);
                    }
                }
                catch (Exception ex)
                {
                    res.Close();
                    ExceptionHandler.ExceptionProcess(ex);
                }

            };
            //httpServer.AddWebSocketService<WsSession>("/live2");
            //httpServer.AddWebSocketService<WsSession>("/ChatWithNyan");
            httpServer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
