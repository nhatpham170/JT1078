using JT1078.Protocol.Enums;
using JT1078.Protocol.H264;
using JT1078.Protocol;
using Microsoft.AspNetCore.SignalR;
using JT1078.Protocol.Extensions;
using JT1078.Flv;
using JT1078NetCore.Flv;

namespace JT1078NetCore.Http
{    
    public class WebSocketService : BackgroundService
    {
        private readonly ILogger<WebSocketService> logger;

        private readonly IHubContext<FlvHub> _hubContext;

        private readonly FlvEncoder fMp4Encoder;

        private readonly WebSocketSession wsSession;

        private readonly H264Decoder h264Decoder;

        public WebSocketService(
            H264Decoder h264Decoder,
            WebSocketSession wsSession,
            FlvEncoder fMp4Encoder,
            ILoggerFactory loggerFactory,
            IHubContext<FlvHub> hubContext)
        {
            this.h264Decoder = h264Decoder;
            logger = loggerFactory.CreateLogger<WebSocketService>();
            this.fMp4Encoder = fMp4Encoder;
            _hubContext = hubContext;
            this.wsSession = wsSession;
        }

        public List<byte[]> q = new List<byte[]>();

        void Init()
        {
            return;
            List<JT1078Package> packages = new List<JT1078Package>();
            var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "H264", "jt1078_6.txt"));
            int mergeBodyLength = 0;
            foreach (var line in lines)
            {
                var bytes = line.ToHexBytes();
                JT1078Package package = JT1078Serializer.Deserialize(bytes);
                mergeBodyLength += package.DataBodyLength;
                var packageMerge = JT1078Serializer.Merge(package);
                if (packageMerge != null)
                {
                    packages.Add(packageMerge);
                }
            }

            var avframe = h264Decoder.ParseAVFrame(packages[0]);
            //q.Add(fMp4Encoder.FirstVideoBox(avframe));

            Queue<Mp4Frame> mp4Frames = new Queue<Mp4Frame>();
            List<NalUnitType> filter = new List<NalUnitType>();
            filter.Add(NalUnitType.SEI);
            filter.Add(NalUnitType.PPS);
            filter.Add(NalUnitType.SPS);
            filter.Add(NalUnitType.AUD);
            foreach (var package in packages)
            {
                List<H264NALU> h264NALUs = h264Decoder.ParseNALU(package);
                if (h264NALUs != null && h264NALUs.Count > 0)
                {
                    Mp4Frame mp4Frame = new Mp4Frame
                    {
                        Key = package.GetKey(),
                        KeyFrame = package.Label3.DataType == JT1078DataType.VideoI
                    };
                    mp4Frame.NALUs = h264NALUs;
                    mp4Frames.Enqueue(mp4Frame);
                }
            }
            while (mp4Frames.TryDequeue(out Mp4Frame frame))
            {
                //q.Add(fMp4Encoder.OtherVideoBox(frame.NALUs, frame.Key, frame.KeyFrame));
            }
        }

        class Mp4Frame
        {
            public string Key { get; set; }
            public bool KeyFrame { get; set; }
            public List<H264NALU> NALUs { get; set; }
        }

        public Dictionary<string, int> flag = new Dictionary<string, int>();

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var session in wsSession.GetAll())
                    {
                        if (flag.ContainsKey(session))
                        {
                            var len = flag[session];
                            if (q.Count <= len)
                            {
                                break;
                            }
                            await _hubContext.Clients.Client(session).SendAsync("video", q[len], stoppingToken);
                            len++;
                            flag[session] = len;
                        }
                        else
                        {
                            await _hubContext.Clients.Client(session).SendAsync("video", q[0], stoppingToken);
                            flag.Add(session, 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "");
                }
                await Task.Delay(60);
            }
        }
    }
}
