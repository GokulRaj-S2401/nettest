using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.BAL.Models
{
    public class ReadingTimeCapture
    {
        public string? ObjectID { get; set; }

        public object? TimeSpentOnDocumentUsers { get; set; }

        public TimeSpentOnDocuments TimeSpentOnDocuments { get; set; }

    }

    public class TimeSpentOnDocuments
    {
        public string? ObjectID { get; set; }

        public string? UniqueIDOfTimeSpentOnDocumentUsers { get; set; }

        public bool? Type { get; set; }

        public string? FromTime { get; set; }

        public string? ToTime { get; set; }


    }
}
