using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using JT1078NetCore.Common;
using JT1078NetCore.Protocol.JT1078;
using JT1078NetCore.Utils;

namespace JT1078NetCore.Socket
{
    public class SocketProcess : MessageToMessageDecoder<string>
    {
        public override bool IsSharable => true;

        protected override void Decode(IChannelHandlerContext context, string message, List<object> output)
        {
            try
            {
                string channelId = context.Channel.Id.ToString();

                if (message != null)
                {
                    SocketSession session;
                    if (!SocketService.Sessions.TryGetValue(channelId, out session))
                    {
                        if (JT1078Define.SessionInfo(message, out session))
                        {
                            session.ChannelId = channelId;
                        }
                    }                  
                    if(session != null && session.Protocol == "JT1078")
                    {
                        new JT1078Filter().MsgProcess(message,ref session);
                    }
                    if(session != null && session.Valid)
                    {
                        SocketService.Sessions[channelId] = session;
                    }                    
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
    }

}
