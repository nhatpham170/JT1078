using JT1078NetCore.Common;
using System.Text;
using System.Xml.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace JT1078NetCore.Http

{
    public class WsService 
    {
        public class WsSession : WebSocketBehavior
        {
            public bool HasHeader = false;
            protected override void OnClose(CloseEventArgs e)
            {
                //if (_name == null)
                //    return;

                //var fmt = "{0} got logged off...";
                //var msg = String.Format(fmt, _name);

                //Sessions.Broadcast(msg);
            }
            public void Writes(byte[] data)
            {
                Send(data);
            }
            protected override void OnOpen()
            {
                //_name = getName();

                //var fmt = "{0} has logged in!";
                //var msg = String.Format(fmt, _name);                
                //Sessions.Broadcast(msg);
                string id = this.ID;
                Global.Ws = this;
                byte[] demo = Encoding.UTF8.GetBytes("FLV");
                this.Send(demo);
            }
            protected override void OnMessage(MessageEventArgs e)
            {
                //var fmt = "{0}: {1}";
                //var msg = String.Format(fmt, _name, e.Data);

                //Sessions.Broadcast(msg);
            }
        }
        public void Init()
        {
            var wssv = new WebSocketServer(5001);

            wssv.AddWebSocketService<WsSession>("/live");
            wssv.Start();            
        }
    }
}
