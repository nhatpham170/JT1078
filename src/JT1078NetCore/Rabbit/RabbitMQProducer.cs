using JT1078NetCore.Utils;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace JT1078NetCore.Rabbit
{
    public class RabbitMQProducer
    {
        private IConnection _conn;        
        private IChannel _channel;
        //private ConnectionFactory _factory;
        //        
        private bool _mandatory = false;
        private const int ReConnectInterval = 15000; // 5 seconds                
        //
        private string RabbitMQExchange = "amq.direct";
        //private string PrefixRoutingKey = string.Empty;
        private string Uri = string.Empty;
        public bool IsConnected = false;
        private int _ttl = 0;
        private int countReconnect = 0;

        /// <summary>        
        /// </summary>
        /// <param name="uri"></param>
        public RabbitMQProducer(string uri)
        {
            Uri = uri;
            Start(uri);
        }

        public RabbitMQProducer()
        {

        }

        private async Task Start(string uri)
        {
            try
            {
                Log.WriteStatusLog(DateTime.Now + $": [RMQ-P] [Start] Try connecting to RabbitMQ Server URI: {Uri}");
                ParseUri(uri);
                Thread.Sleep(1000);
                var factory = new ConnectionFactory();
                factory.AutomaticRecoveryEnabled = true;
                factory.RequestedHeartbeat = TimeSpan.FromSeconds(5);
                factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
                factory.TopologyRecoveryEnabled = true;
                factory.Uri = new Uri(uri);

                countReconnect = 0;
            INIT_CONNECTION:;
                try
                {
                    // init connection
                    _conn = await factory.CreateConnectionAsync();
                    _conn.ConnectionShutdownAsync += EventConnectionShutdown;
                    _conn.RecoverySucceededAsync += EventRecoverySucceeded;
                    IsConnected = true;
                    _channel = await _conn.CreateChannelAsync();
                    //props = _channel..CreateBasicProperties();
                    //props.ContentType = "text/plain";
                    //props.DeliveryMode = DeliveryModes.Transient;
                    //if (_ttl > 0)
                    //{
                    //    props.Expiration = _ttl.ToString();
                    //}
                    Log.WriteStatusLog(DateTime.Now + $": [RMQ-P] [Start] Connect to RabbitMQ Server Success Uri: {Uri}");
                }
                catch (BrokerUnreachableException e)
                {
                    countReconnect += 1;
                    Log.WriteStatusLog(DateTime.Now + $": [RMQ-P] [Start] Connect to RabbitMQ Server Failed Uri: {Uri}");
                    if (countReconnect < 3)
                    {
                        ExceptionHandler.ExceptionProcess(e);
                    }
                    Thread.Sleep(ReConnectInterval);
                    goto INIT_CONNECTION;
                }
                catch (Exception ex)
                {
                    countReconnect += 1;
                    Log.WriteStatusLog(DateTime.Now + ": [RMQ-P] [Start] Failed to connect to RabbitMQ Server, Number of Reconnect : " + countReconnect.ToString());
                    if (countReconnect < 3)
                    {
                        ExceptionHandler.ExceptionProcess(ex);
                    }
                    Thread.Sleep(ReConnectInterval);
                    goto INIT_CONNECTION;
                }
            }
            catch (Exception ex)
            {
                Log.WriteStatusLog(DateTime.Now + $": Failed in connecting to RabbitMQ Server. URI: {this.Uri}");
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        private async Task EventRecoverySucceeded(object sender, AsyncEventArgs @event)
        {
            try
            {
                IsConnected = true;
                Log.WriteStatusLog($"{DateTime.Now}: [RMQ-P] [RecoverySucceeded] RMQ Uri: {Uri}");
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        private async Task EventConnectionShutdown(object sender, ShutdownEventArgs @event)
        {
            try
            {
                IsConnected = false;
                Log.WriteStatusLog($"{DateTime.Now}: [RMQ-P] [ConnectionShutdown] RMQ Uri: {Uri}");
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        //private void EventConnectionShutdown(object sender, ShutdownEventArgs e)
        //{
        //    try
        //    {
        //        IsConnected = false;
        //        Log.WriteStatusLog($"{DateTime.Now}: [RMQ-P] [ConnectionShutdown] RMQ Uri: {Uri}");
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionHandler.ExceptionProcess(ex);
        //    }
        //}
        //private void EventRecoverySucceeded(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        IsConnected = true;
        //        Log.WriteStatusLog($"{DateTime.Now}: [RMQ-P] [RecoverySucceeded] RMQ Uri: {Uri}");
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionHandler.ExceptionProcess(ex);
        //    }
        //}

        public void Stop()
        {
            try
            {

                if (_channel != null)
                {
                    _channel.CloseAsync();
                }
                if (_conn != null)
                {
                    _ = _conn.CloseAsync();
                    Log.WriteStatusLog(DateTime.Now + $": [RMQ-P] Stopped connection Uri:{Uri} ...");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }

        private void ParseUri(string uri)
        {
            // process Uri
            try
            {
                string[] arr = uri.Split('?');
                int ttl = 0;
                if (arr.Length > 1)
                {
                    string paramStr = arr[1].Replace(';', '&');
                    NameValueCollection param = HttpUtility.ParseQueryString(paramStr);
                    int.TryParse(param.Get("ttl"), out ttl);
                    RabbitMQExchange = param.Get("exchange").ToString();
                    //PrefixRoutingKey = param.Get("prefix").ToString();
                    //string ttlStr = param.Get("ttl").ToString();
                    //if (int.TryParse(ttlStr, out ttl))
                    //{
                    //    this._ttl = ttl;
                    //}
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }     
        //public bool putData(object dataStr, string serialNumber)
        //{
        //    string routingKey = "";
        //    try
        //    {
        //        if (IsConnected)
        //        {
        //            routingKey = this.PrefixRoutingKey + serialNumber.Substring(serialNumber.Length - 1, 1);
        //            this._channel.BasicPublishAsync(RabbitMQExchange, routingKey, Encoding.UTF8.GetBytes((string)dataStr), CancellationToken.None);
        //            return true;
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        try
        //        {
        //            Log.WriteExceptionLog($"{DateTime.Now}: dataStr => {dataStr} serialNumber => {serialNumber}; routingKey => {routingKey}");

        //        }
        //        catch (Exception ex)
        //        {
        //            ExceptionHandler.ExceptionProcess(ex);
        //        }
        //        this.ProcessException(exception);
        //    }
        //    return false;
        //}
        public async Task<bool> put(string routingKey, object obj)
        {

            try
            {
                if (IsConnected)
                {                    
                    await _channel.BasicPublishAsync(this.RabbitMQExchange, routingKey, Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj)));
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.ProcessException(exception);
            }
            return false;
        }

      
        private void ProcessException(Exception exception)
        {
            try
            {
                //Already closed: The AMQP operation was interrupted: AMQP close-reason
                if (exception.Message.Contains("Already closed"))
                {
                    IsConnected = false;
                }
                ExceptionHandler.ExceptionProcess(exception);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ExceptionProcess(ex);
            }
        }
    }
}
