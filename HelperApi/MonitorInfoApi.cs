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
            public List<Item> item { get; set; }
        }

        public class MonitorInfo
        {
            public Xml xml { get; set; }
            public MonitorsList monitors_list { get; set; }
        }


    }
}
