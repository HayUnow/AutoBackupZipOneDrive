
using System;

namespace AutoBackupZipOneDrive.Models
{
    public class AppConfig
    {
        public string MonitorPath { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int ScanIntervalSeconds { get; set; } = 30;
        public int StableSeconds { get; set; } = 30;
        public int WindowMinutes { get; set; } = 5;
        public string Password { get; set; } = "";

        public string OneDrivePath { get; set; } = "";
        public int ZipTempKeepDays { get; set; } = 7;
    }
}
