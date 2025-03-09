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
                HexUtil hex = new HexUtil(msg);
                hex.Skip(4); // header 
                int lable1 = hex.ReadByte();
                int lable2 = hex.ReadByte();
                hex.ReadInt16();
                session.Imei = hex.ReadBCD();
                session.Chl = hex.ReadByte() & 0x0F;
                session.Protocol = "JT1078";
                session.Valid = true;
                return true;
            }
            return false;
        }
    }
}
