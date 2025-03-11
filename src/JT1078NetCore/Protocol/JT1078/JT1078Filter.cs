using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using JT1078.Protocol.Enums;
using JT1078.Protocol;
using JT1078NetCore.Common;
using JT1078NetCore.Utils;
using JT1078NetCore.Socket;
using JT1078.Protocol.Extensions;
using JT1078.Flv;
using System.IO;

namespace JT1078NetCore.Protocol.JT1078
{   
    public class JT1078Filter
    {
        public void MsgProcess(string message , ref SocketSession session)
        {
            try
            {                                
                if (message != null)
                {
                    string hex = session.Reverse + message;
                    List<string> listDataMedia = new List<string>();
                    string fragment = string.Empty;                    
                    ProcessFileFilterPackageJT1078New(hex, out listDataMedia, out fragment);
                    session.Reverse = fragment;
                    FlvEncoder encoder = new FlvEncoder();
                    foreach (string item in listDataMedia)
                    {                        
                        JT1078Package package = JT1078Serializer.Deserialize(item.ToHexBytes());
                        JT1078Package fullpackage = JT1078Serializer.Merge(package);
                        if(fullpackage != null)
                        {
                            //if(Global.Ws != null )
                            //{
                            //    //ulong timeNow = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            //    //if (Global.Ws.StartTime == 0)
                            //    //{
                            //    //    Global.Ws.StartTime = timeNow;
                            //    //    Global.Ws.LastTime = timeNow;
                            //    //}
                            //    //fullpackage.Timestamp = fullpackage.Timestamp - 500;
                            //    //fullpackage.LastFrameInterval = (ushort)(fullpackage.LastFrameInterval - 100);
                            //    //fullpackage.Timestamp = (ulong)(timeNow - Global.Ws.StartTime);
                            //    //fullpackage.LastFrameInterval = (ushort)(timeNow - Global.Ws.LastTime);
                            //    //Global.Ws.LastTime = timeNow;
                                
                            //}
                            if (session.FlvHeader == null)
                            {
                                byte[] flvHeaderTag = encoder.EncoderFlvHeader(fullpackage);
                                session.FlvHeader = flvHeaderTag;
                            }
                            var videoTag = encoder.EncoderVideoTag(fullpackage, false);
                            session.LastFrame = videoTag;
                            session.Broadcast(videoTag);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        private void ProcessFileFilterPackageJT1078New(string hex, out List<string> listData, out string fragment)
        {
            //30316364 //FH_Flag
            //81 // Label1
            //06 // Label2
            //0001 // SN
            //015000085960 // SIM
            //01 // LogicChannelNumber
            //30 // Label3
            //000001912BEF9AC1 // Timestamp
            //0140 // DataBodyLength
            //57505E50D7D8D9D6D5D4D0D1D2DCD2D6D5D65C4059D5DFD9DED75D585B5F55DDD8DDD55653535D52D7DBC7D3D4D1D754505F44445FD7DFD1535257D6D9C5DCD6545454D6D7D1555E5D53585AD4C6C1DD57454153D4D5DFD2515C5A5DD2DAC6CDC4565A5B5B5CD3DCD3D6D6D1D6DDDCD6D1565B5B59454150DDD3DCDD5C4C5BD8C1D8D3D558445D5D57D0D1D9CBCFD7525C5257D55356D0D0DD545957DCDDD557555455D3C5C1D9545E5B5D535E5154565F5CD1D45250D1DEDED55E56D1515350535DD5DFD55351DDDED0D9C1DD595ED4D1D2DCD3545057525A5B57D6D4D4D1D5D557575556D3DBD2D4D3D751D5D6525B5C595ED4D1D1DD555F5DD6D352D5C5C6DDD5D3D2D65347455E54DFD2575651585ED6DFD1D6555E57DCDDD9DF505B53D655D0D6535357DCC4DBDC555D505C5E56545755D3D3D2DFD154505755D5D55359            
            listData = new List<string>();
            fragment = string.Empty;
            try
            {
                string START = "30316364";
                // write text
                string str = hex;
                fragment = str;
                int count = 0;
                while (str.Length > 52)
                {
                    try
                    {
                        bool isVIdeo = false;
                        int index = str.IndexOf(START);
                        if (index >= 0 && str.Length >= 80)
                        {
                            JT1078Label3 label3 = new JT1078Label3(Convert.ToByte(str.Substring(index + 30, 2), 0x10));
                            // find package 
                            int indexLength = index + 32;
                            if (label3.DataType != JT1078DataType.TransparentData)
                            {
                                indexLength += 16;
                                // get time
                            }
                            if (label3.DataType != JT1078DataType.TransparentData && label3.DataType != JT1078DataType.Audio)
                            {
                                indexLength += 8;
                                isVIdeo = true;
                            }
                            //if(label3.DataType == JT1078DataType.Audio)
                            //{
                            //    isVIdeo = true;
                            //}

                            int length = Convert.ToInt16(str.Substring(indexLength, 4), 0x10) * 2;
                            if (length > 1908)
                            {
                                length = 1908;
                            }
                            int fullLength = length + indexLength + 4;

                            if (index + fullLength <= str.Length)
                            {
                                string fullPackage = str.Substring(index, fullLength);
                                if (isVIdeo)
                                {
                                    // video
                                    listData.Add(fullPackage);
                                }
                                str = str.Substring(index + fullPackage.Length);
                                fragment = str;
                                count++;

                            }
                            else { break; }
                        }
                        else { break; }
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.ExceptionProcess(ex);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.ExceptionProcess(e);
            }
        }
    }
}
