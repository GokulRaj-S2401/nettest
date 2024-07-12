using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZOI.BAL.Utilities.Constants;

namespace ZOI.BAL.Models
{
    internal class ErrorLog
    {
        public string? Message { get; set; }
        public string? StkStrace { get; set; }
        public string? MethodName { get; set; }
        public string CreatedOn { get; set; } = DateTime.Now.ToString(DateTimeFormat.DateTimeWith24HrsFormat);
    }
}
