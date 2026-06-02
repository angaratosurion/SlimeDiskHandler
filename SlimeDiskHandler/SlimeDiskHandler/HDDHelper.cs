
using System.Management;
namespace SlimeDiskHandler
{
    public static class HDDHelper
    {
        public static string? GetPhysicalDrive(string driveLetter)
        {
            driveLetter = driveLetter.TrimEnd('\\');

            using var searcher =
                new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} " +
                    "WHERE AssocClass=Win32_LogicalDiskToPartition");

            foreach (ManagementObject part in searcher.Get())
            {
                using var diskSearch =
                    new ManagementObjectSearcher(
                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{part["DeviceID"]}'}} " +
                        "WHERE AssocClass=Win32_DiskDriveToDiskPartition");

                foreach (ManagementObject disk in diskSearch.Get())
                    return disk["DeviceID"]?.ToString();
            }

            return null;
        }
    }
    }
