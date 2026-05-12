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
        [DefaultValue(null)]
        [JsonProperty("AdditionalParameters", DefaultValueHandling = DefaultValueHandling.Populate)]
        public AdditionalParameters AdditionalParameters { get; set; }
    }

    public class AdditionalParameters
    {
        [JsonProperty("label", Required = Required.Always)]
        public string Label { get; set; }
    }
}
