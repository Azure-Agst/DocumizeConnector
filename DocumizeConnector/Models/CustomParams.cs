using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumizeConnector.Models
{
    public class CustomParams
    {
        [JsonProperty("label", Required = Required.Always)]
        public string Label { get; set; }

        override public string ToString() { 
            return "{'Label':'" + Label + "'}"; 
        }
    }
}
