using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.BAL.Models
{
    public class FileUpload
    {
        public string? objectID { get; set; }

        public string? pageName { get; set; }

        public dynamic? imageList { get; set; }

        public bool? isAdd { get; set; }

        public string? imageID { get; set; }
    }
}
