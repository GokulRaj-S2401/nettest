using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.BAL.Models
{
    public class Variable
    {
        public string? objectID { get; set; }
        public string? variableName { get; set; }
        public string? variableValue { get; set; }
        public bool isApplyAll { get; set; }
        public object group { get; set; }
    }
}
