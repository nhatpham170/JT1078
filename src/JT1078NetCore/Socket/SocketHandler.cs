using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using JT1078NetCore.Utils;
using System.Reflection.Emit;

namespace JT1078NetCore.Socket
{  
    public class SocketHandler : SimpleChannelInboundHandler<object>
    {
        public override Task CloseAsync(IChannelHandlerContext context)
        {
            return base.CloseAsync(context);
        }
        public override void ChannelActive(IChannelHandlerContext context)
        {
            try
            {
                Global.DictChannels[context.Channel.Id.ToString()] = context.Channel;
            }
            catch (Exception ex)
            {

                ExceptionHandler.ExceptionProcess(ex);
            }
        }
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            try
            {
                string channelId = context.Channel.Id.ToString();
                
                SocketSession session;
                if (Global.SESSIONS_CHANNEL.TryRemove(channelId, out session))
                {
                    if(session.ChannelId == channelId)
                    {
                        session.Destroy();
                    }
                }
                Global.DictChannels.TryRemove(channelId, out _);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
            finally
            {
                base.ChannelInactive(context);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext contex)
        {
            try
            {
                contex.Flush();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            try
            {
                contex.CloseAsync();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {

        }

        public override bool IsSharable => true;
    }
}
