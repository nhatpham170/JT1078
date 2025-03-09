using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using JT1078.Protocol.Enums;
using JT1078.Protocol;
using JT1078NetCore.Common;
using JT1078NetCore.Utils;

namespace JT1078NetCore.Protocol.Howen
{
    public class HowenFilter : MessageToMessageDecoder<string>
    {
        public override bool IsSharable => true;

        protected override void Decode(IChannelHandlerContext context, string message, List<object> output)
        {
            try
            {
                string channelId = context.Channel.Id.ToString();

                if (message != null)
                {
                    string buffer = "";
                    Global.DictBuffer.TryGetValue(channelId, out buffer);
                    string hex = message;
                    if (buffer != null)
                    {
                        hex = buffer + message;

                    }
                    List<BasePackage> listDataMedia = new List<BasePackage>();
                    string fragment = string.Empty;
                    FilterPackageHowenMDVR(hex, out listDataMedia, out fragment);
                    Global.DictBuffer[channelId] = fragment;
                    if (listDataMedia.Count > 0)
                    {
                        for (int i = 0; i < listDataMedia.Count; i++)
                        {
                            output.Add(listDataMedia[i]);
                            Log.WriteFile(listDataMedia[i].StrHex, "rpt_filter");
                        }
                    }
                    message = null;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        private void ProcessFilterPackageHowenMDVR(string hex, out List<string> listData, out string fragment)
        {
            fragment = hex;
            listData = new List<string>();
            if (hex.Length > 16)
            {

            }

        }
        public struct BasePackage
        {
            public string Version;
            public string Type;
            public string IMEI;
            public string Session;
            public string Content;
            public object ContentObj;
            public string StrHex;
        }
        public void FilterPackageHowenMDVR(string strHex, out List<BasePackage> listData, out string fragment)
        {
            listData = new List<BasePackage>();
            //List<BasePackage> result = new List<BasePackage>();
            if (string.IsNullOrEmpty(strHex))
            {
                strHex = string.Empty;
            }
            HexUtil hex = new HexUtil(strHex);
            fragment = strHex;
            const string HEAD = "48";
            try
            {

                bool flag = true;
                while (flag)
                {
                    if (hex.Remain() >= 8)
                    {
                        int index = hex.FindIndex(HEAD);

                        if (index >= 0)
                        {
                            hex.Reset(index);
                            hex.Skip(2); // version
                            string type = hex.ReadBytes(2, true);
                            int length = hex.ReadInt32(true);
                            if (hex.Remain() >= length)
                            {
                                string content = hex.ReadBytes(length);
                                string fullPackage = hex.ReadStart(0, 8 + length);
                                fragment = hex.Copy(0);
                                BasePackage package = new BasePackage();
                                package.Content = content;
                                package.Type = type;
                                package.StrHex = fullPackage;

                                //result.Add(package);
                                listData.Add(package);
                                hex.Release();
                            }
                            else break;
                        }
                        else break;
                    }
                    else break;
                }
                if (listData.Count == 0)
                {
                    fragment = strHex;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
    }
}
