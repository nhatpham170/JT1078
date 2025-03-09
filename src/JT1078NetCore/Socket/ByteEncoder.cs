using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace JT1078NetCore.Socket
{
    public class ByteEncoder : MessageToMessageEncoder<byte[]>
    {
        public override bool IsSharable => true;

        protected override void Encode(IChannelHandlerContext context, byte[] message, List<object> output)
        {
            try
            {
                if (message != null)
                {
                    byte[] ba = message;
                    IByteBuffer heapBuffer = context.Allocator.HeapBuffer(ba.Length);
                    heapBuffer.WriteBytes(ba, 0, ba.Length);
                    output.Add(heapBuffer);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionHandler.ExceptionProcess(ex);
            }

        }
    }
}
