using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumizeConnector.Models
{
    internal class DocuAuth
    {
        [JsonProperty("bearer", Required = Required.Always)]
        public string Bearer { get; set; }
    }
}
