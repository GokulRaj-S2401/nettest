using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.BAL.Models
{
    public class Document
    {
        public string? ObjectID { get; set; }
        public string? CollectionName { get; set; }
        public string? UserID { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByID { get; set; }

        public List<DocumentDetails> Content { get; set; }

    }

    public class DocumentDetails
    {
        public string? RowID { get; set; }
        public string? BlockID { get; set; }
        public string? Value { get; set; }
    }

}
