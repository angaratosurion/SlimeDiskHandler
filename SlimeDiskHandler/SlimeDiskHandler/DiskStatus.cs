using System;
using System.Collections.Generic;
using System.Text;

namespace SlimeDiskHandler
{
    public class DiskStatus
    {
        public string Drive { get; set; } = "";
        public int? Temp { get; set; }
        public bool Idle { get; set; }
    }
}
