using System;
using System.Collections.Generic;
using System.Text;

namespace SlimeDiskHandler
{
    public class DiskProfile
    {
        public string DriveLetter { get; set; } = "Y:";
        public int TempThreshold { get; set; } = 50;
        public int IdleTimeMinutes { get; set; } = 10;
        public string SmartctlPath { get; set; } = "smartctl.exe";
        public string PowerOffCmd { get; set; } = "HotSwap!.EXE";
        public bool PowerOffEvenWehnUsed { get; set; }
        public bool EnableLogging { get; set; }
    }
}

