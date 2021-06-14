using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamestreamLauncher.HelperApi
{
    class MonitorInfoApi
    {
        public class Xml
        {
            public string version { get; set; }
        }

        public class Item
        {
            public string resolution { get; set; }
            public string lefttop { get; set; }
            public string rightbottom { get; set; }
            public string active { get; set; }
            public string disconnected { get; set; }
            public string primary { get; set; }
            public string colors { get; set; }
            public string frequency { get; set; }
            public string orientation { get; set; }
            public string maximum_resolution { get; set; }
            public string name { get; set; }
            public string adapter { get; set; }
            public string device_id { get; set; }
            public string device_key { get; set; }
            public string monitor_id { get; set; }
            public string monitor_key { get; set; }
            public string monitor_string { get; set; }
            public string monitor_name { get; set; }
            public string monitor_serial_number { get; set; }
        }

        public class MonitorsList
        {
            [JsonConverter(typeof(SingleOrArrayConverter<Item>))]
            public List<Item> item { get; set; }
        }

        public class MonitorInfo
        {
            public Xml xml { get; set; }
            public MonitorsList monitors_list { get; set; }
        }

        class SingleOrArrayConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(List<T>));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken token = JToken.Load(reader);
                if (token.Type == JTokenType.Array)
                {
                    return token.ToObject<List<T>>();
                }
                return new List<T> { token.ToObject<T>() };
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
