using JT1078.Protocol;
using JT1078.Protocol.Extensions;
using JT1078.Protocol.H264;
using JT1078.Protocol.MessagePack;
using JT1078NetCore.Common;
using JT1078NetCore.Socket;
using JT1078NetCore.Utils;

namespace JT1078NetCore.Protocol.JT1078
{
    public class JT1078Define
    {
        private const string HEADER = "30316364";
        public enum DataType:byte
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
                session.StreamType = 0;
                session.Protocol = "JT1078";
                session.Valid = true;
                JT1078Package package = JT1078Serializer.Deserialize(msg.ToHexBytes());
                H264Decoder h264Decoder = new H264Decoder();
                var nalus = h264Decoder.ParseNALU(package);

                var sps = nalus.FirstOrDefault(x => x.NALUHeader.NalUnitType == NalUnitType.SPS);
                if (sps != null) {
                    var rawData = h264Decoder.DiscardEmulationPreventionBytes(sps.RawData);
                    ExpGolombReader h264GolombReader = new ExpGolombReader(rawData);
                    SPSInfo spsInfo = h264GolombReader.ReadSPS();
                    if (spsInfo.height >= 720) {
                        session.StreamType = 0;
                    }else
                    {
                        session.StreamType = 1;
                    }
                    //if(spsInfo.levelIdc >= 31 && spsInfo.profileIdc == 100)
                    //{
                    //    session.StreamType = 0; 
                    //}
                    //else
                    //{
                    //    session.StreamType = 1;
                    //}                    
                }                
                return true;
            }
            return false;
        }
    }
}
