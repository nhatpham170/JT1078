using Newtonsoft.Json;

namespace JT1078NetCore.Common.Models
{
    
    [Serializable]
    public class ApiEvent
    {
        public class ActionTypes
        {
            public const string CREATE = "create";
        }
        public class ObjTypes
        {
            public const string COMMAND = "command";
        }
        [JsonProperty("actionType")]
        public string ActionType { get; set; }
        [JsonProperty("objType")]
        public string ObjType { get; set; }
        [JsonProperty("objOld")]
        public Object ObjOld { get; set; }
        [JsonProperty("objNew")]
        public Object ObjNew { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        public ApiEvent()
        {
            this.ActionType = ActionTypes.CREATE;
            this.ObjType = ObjTypes.COMMAND;
            this.Timestamp = 0;
        }
    }
}
