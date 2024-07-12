using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZOI.BAL.Utilities.Constants;

namespace ZOI.BAL.Models
{
    public class UserInvitation
    {
        public int? Id { get; set; } 
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string ExpiredTill { get; set; } = DateTime.Now.AddDays(7).ToString(DateTimeFormat.DateTimeWith24HrsFormat);
        public bool IsActive { get; set; } = true;
        public bool IsExpired { get; set; } = false;
        public string? ExpiredOn { get; set; }
        public string CreatedOn { get; set; } = DateTime.Now.ToString(DateTimeFormat.DateTimeWith24HrsFormat);

    }
}
