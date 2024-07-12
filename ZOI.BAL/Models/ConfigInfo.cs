using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceMonitoring.Models
{
    public class ConfigInfo
    {
        public ConfigInformation? ConfigInformation { get; set; }

        public Drive? Drive { get; set; }
            
        public Database? Database { get; set; }

        public Utilization? Others { get; set; }
    }

    public class ConfigInformation
    {
        public bool ReferLocalConf { get; set; }

        public string? ConfigFile { get; set; }

        public string? ConfigSection { get; set; }
    }

    public class Database
    {
        public string? HostName { get; set; }

        public string? DatabaseName { get; set; }

        public string? DBUser { get; set; }

        public string? Password { get; set; }

        public string? Status { get; set; }

        public string? ErrMessage { get; set; }
    }

    public class Drive
    {
        public string? Folder { get; set; }

        public string? ToCheckFor { get; set; }

        public string? Status { get; set; }

        public string? ErrMessage { get; set; }
    }

    public class Utilization
    {
        public bool ChkCPU { get; set; }

        public string CPUUtilization { get; set; } = "0% CPU";

        public bool ChkRAM { get; set; }

        public string RAMUtilization { get; set; } = "0%";

        public bool ChkNetwork { get; set; }

        public string NetworkUtilization { get; set; } = "0 kb/s";
    }

}
