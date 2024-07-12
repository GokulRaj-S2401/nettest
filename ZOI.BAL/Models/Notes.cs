using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.BAL.Models
{
    public class Notes
    {
        public string? rowID { get; set; }
        public string? blockID { get; set; }
        public object? notesID { get; set; }
        public string? note { get; set; }
        public dynamic? Data { get; set; }
        public string? objectID { get; set; }
        public bool? IsAdd { get; set; }
    }
}
