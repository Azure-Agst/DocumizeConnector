using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumizeConnector.Models
{
    public class Page
    {
        [Key]
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Always)]
        public string ID { get; set; }

        [JsonProperty("body", Required = Required.Always)]
        public string Body { get; set; }

        [JsonProperty("created", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [DefaultValue(null)]
        [JsonProperty("revised", DefaultValueHandling = DefaultValueHandling.Populate)]
        public DateTime UpdatedAt { get; set; }

    }
}
