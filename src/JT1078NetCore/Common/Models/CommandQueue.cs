using Newtonsoft.Json;

namespace JT1078NetCore.Common.Models
{
    [Serializable]
    public class CommandQueue
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("deviceId")]
        public long DeviceID { get; set; }
        [JsonProperty("imei")]
        public string Imei { get; set; }
        [JsonProperty("commandCode")]
        public long CommandCode { get; set; }
        //public string Key { get; set; }
        [JsonProperty("commandName")]
        public string CommandName { get; set; }
        [JsonProperty("commandText")]
        public string CommandText { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreateDate { get; set; }
        [JsonProperty("isSend")]
        public string IsSend { get; set; }
        //public DateTime SendDate { get; set; }
        //public bool? IsResponse { get; set; }
        //public DateTime ResponseDate { get; set; }
        //public string ResponseText { get; set; }
        [JsonProperty("isOfflineSend")]
        public string IsOfflineSend { get; set; }
        //[JsonProperty("valid")]
        //public bool? Valid { get; set; }
        //public bool SkipResponse { get; set; }

        //public string PackageToSend { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("timeFrom")]
        public long TimeFrom { get; set; }
        [JsonProperty("timeTo")]
        public long TimeTo { get; set; }
        //public int TimeDelay { get; set; }
        //public int Timeout { get; set; } // seconds
        //public int MultiRes { get; set; }
        
        public int IdAuto()
        {
            ID = Random.Shared.Next(9999,99999999) * -1;            
            return ID;
        }
    }
}
