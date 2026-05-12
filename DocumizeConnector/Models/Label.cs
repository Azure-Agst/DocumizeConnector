using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumizeConnector.Models
{
    internal class Label
    {
        [Key]
        public string ID { get; set; }
        public string Name { get; set; }
    }
}
