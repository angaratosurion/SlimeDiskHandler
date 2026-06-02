using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SlimeDiskHandler
{

    public static class DiskIdleChecker
    {
        public static bool IsIdle(string physicalDrive, long threshold, int seconds)
        {
            var instance = GetInstance(physicalDrive);
            if (instance == null) return true;

            var counter = new PerformanceCounter(
                "PhysicalDisk",
                "Disk Bytes/sec",
                instance,
                true);

            int idleCount = 0;

            for (int i = 0; i < seconds; i++)
            {
                float val = counter.NextValue();
                Thread.Sleep(1000);

                if (val <= threshold)
                    idleCount++;
                else
                    idleCount = 0;
            }

            return idleCount >= seconds;
        }

        private static string? GetInstance(string physicalDrive)
        {
            var m = System.Text.RegularExpressions.Regex.Match(
                physicalDrive,
                @"PHYSICALDRIVE(\d+)");

            if (!m.Success) return null;

            return m.Groups[1].Value + " C:";
        }
    }
}
