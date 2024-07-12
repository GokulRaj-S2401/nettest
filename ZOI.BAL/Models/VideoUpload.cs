using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.BAL.Models
{
    public  class VideoUpload

    {
        public string? ObjectID { get; set; }

        public string? folderName { get; set; }

        public dynamic? videoList { get; set; }

        public bool? isAdd { get; set; }

        public string? videoId { get; set; }

        public string? folder { get; set; }
    }
}
