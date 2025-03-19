using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using JT1078NetCore.Utils;

namespace JT1078NetCore.Socket
{
    public class HexDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        public override bool IsSharable => true;

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            try
            {
                string channelId = context.Channel.Id.ToString();

                if (message != null)
                {
                    message.MarkReaderIndex();
                    int stringLength = message.ReadableBytes;
                    var b = new byte[stringLength];
                    message.GetBytes(0, b, 0, stringLength);
                    string hex = BitConverter.ToString(b).Replace("-", "");                    
                    
                    if (!string.IsNullOrEmpty(hex))
                    {                        
                        output.Add(hex);
                        //Log.WriteDeviceLog(hex, channelId);
                    }
                    message = null;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
    }
}
