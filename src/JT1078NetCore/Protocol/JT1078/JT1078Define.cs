using JT1078.Protocol;
using JT1078.Protocol.Extensions;
using JT1078.Protocol.H264;
using JT1078.Protocol.MessagePack;
using JT1078NetCore.Common;
using JT1078NetCore.Socket;
using JT1078NetCore.Utils;
using Newtonsoft.Json.Linq;
using static JT1078NetCore.Common.MediaDefine;

namespace JT1078NetCore.Protocol.JT1078
{
    public class JT1078Define
    {
        private const string HEADER = "30316364";
        public enum DataType : byte
        {
            VideoI = 0,
            VideoP = 1,
            VideoB = 2,
            Audio = 3,
            TransparentData = 4,
        }

        public static bool IsProtocol(string message)
        {
            return message.StartsWith(HEADER);
        }

        public static bool SessionInfo(string msg, out SocketSession session)
        {
            session = new SocketSession();
            if (IsProtocol(msg))
            {
                Utils.HexUtil hex = new Utils.HexUtil(msg);
                hex.Skip(4); // header 
                int lable1 = hex.ReadByte();
                int lable2 = hex.ReadByte();
                hex.ReadInt16();
                session.Imei = hex.ReadBCD();
                session.Chl = hex.ReadByte() & 0x0F;
                session.PlayType = MediaDefine.PlayType.Live;
                session.StreamType = MediaDefine.StreamType.Sub;
                session.Protocol = "JT1078";
                session.Valid = true;
                string keySub = $"{session.PlayType}_{session.Imei}_{session.Chl}_{(int)StreamType.Sub}";
                string keyMain = $"{session.PlayType}_{session.Imei}_{session.Chl}_{(int)StreamType.Main}";
                string keyPlaybackSub = $"{PlayType.Playback}_{session.Imei}_{session.Chl}_{(int)StreamType.Sub}";
                string keyPlaybackMain = $"{PlayType.Playback}_{session.Imei}_{session.Chl}_{(int)StreamType.Main}";                
                string keyPlayback = $"{PlayType.Playback}_{session.Imei}_{session.Chl}";                
                SocketSession sessionPendding = new SocketSession();                
                if (Global.SESSIONS_MAIN.TryGetValue(keySub, out sessionPendding) && !sessionPendding.IsConnected)
                {
                    session.StreamType = StreamType.Sub;
                    session.PlayType = PlayType.Live;
                    session.SetKey(keySub);                    
                }
                else if (Global.SESSIONS_MAIN.TryGetValue(keyMain, out sessionPendding) && !sessionPendding.IsConnected)
                {
                    session.StreamType = StreamType.Main;
                    session.PlayType = PlayType.Live;
                    session.SetKey(keyMain);
                }
                else
                {
                    // check playback
                    var playback = Global.SESSIONS_MAIN.First(x => x.Key.StartsWith(keyPlayback) && !x.Value.IsConnected);
                    if(playback.Value != null)
                    {
                        sessionPendding = playback.Value;
                        session.StreamType = sessionPendding.StreamType;
                        session.PlayType = sessionPendding.PlayType;
                        session.SetKey(sessionPendding.Key);
                    }

                }
                //if (Global.SESSIONS_MAIN.TryGetValue(keyPlaybackSub, out sessionPendding) && !sessionPendding.IsConnected)
                //{
                //    session.StreamType = StreamType.Sub;
                //    session.PlayType = PlayType.Playback;
                //    session.SetKey(keyPlaybackSub);
                //}
                //else if (Global.SESSIONS_MAIN.TryGetValue(keyPlaybackMain, out sessionPendding) && !sessionPendding.IsConnected)
                //{
                //    session.StreamType = StreamType.Main;
                //    session.PlayType = PlayType.Playback;
                //    session.SetKey(keyPlaybackMain);
                //}      
                //else
                //{
                //    return false;
                //}
                //else
                //{
                //    JT1078Package package = JT1078Serializer.Deserialize(msg.ToHexBytes());
                //    H264Decoder h264Decoder = new H264Decoder();
                //    var nalus = h264Decoder.ParseNALU(package);

                //    var sps = nalus.FirstOrDefault(x => x.NALUHeader.NalUnitType == NalUnitType.SPS);
                //    if (sps != null)
                //    {
                //        var rawData = h264Decoder.DiscardEmulationPreventionBytes(sps.RawData);
                //        ExpGolombReader h264GolombReader = new ExpGolombReader(rawData);
                //        SPSInfo spsInfo = h264GolombReader.ReadSPS();
                //        if (spsInfo.height >= 720)
                //        {
                //            session.StreamType = MediaDefine.StreamType.Main;
                //        }
                //        //if(spsInfo.levelIdc >= 31 && spsInfo.profileIdc == 100)
                //        //{
                //        //    session.StreamType = 0; 
                //        //}
                //        //else
                //        //{
                //        //    session.StreamType = 1;
                //        //}                    
                //    }
                //}

                return true;
            }
            return false;
        }
    }
}
