using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SlimeDiskHandler
{
    public static class SmartHelper
    {
        public static int? GetTemp(string physicalDrive, string smartctl)
        {
            bool driveexsit=false;
            var drives = DriveInfo.GetDrives();
               foreach(var drive in drives)
            {
                if ( drive.Name==physicalDrive )
                {
                    physicalDrive = drive.Name;
                    driveexsit=true;
                    break;
                }
            }
            var output = Run(smartctl, $"-A {physicalDrive}");

            //var match = Regex.Match(output, @"Temperature_Celsius*?(\d+)$", RegexOptions.Multiline);
            // var match = Regex.Match(output, @"Temperature_Celsius\s+\S+\s+\S+\s+\S+\s+\S+\s+\S+\s+\S+\s+(\d+)");
            int temp = (int)ParseTemperature(output);
            return temp;
            //if (match.Success && int.TryParse(match.Groups[1].Value, out int t))
            //    return t;

            return null;
        }
        private static int? ParseTemperature(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return null;

            foreach (var line in output.Split('\n'))
            {
                if (!line.Contains("Temperature"))
                    continue;

                // priority 1: attribute 194 (most common HDD)
                if (line.TrimStart().StartsWith("194"))
                {
                   var line2= line.Substring(line.IndexOf(" - ")+1,
                       line.IndexOf("(")-line.IndexOf(" - ")-1);
                    var temp = ExtractFirstNumber(line2);
                    if (temp.HasValue) return temp;
                }

                // fallback: any temperature line
                //if (line.Contains("Celsius"))
                //{
                //    var temp = ExtractFirstNumber(line);
                //    if (temp.HasValue) return temp;
                //}
            }

            return null;
        }
        private static int? ExtractFirstNumber(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;
           line= line.Replace("-","");
            foreach (var token in line.Split(' ',
                StringSplitOptions.RemoveEmptyEntries))
            {
                //if (token.Contains("("))
                {
                    var token2 = token;//.Substring(0, token.IndexOf('('));


                    if (int.TryParse(token2, out int value))
                        return value;
                }
            }
        

            return null;
         }


        public static void Standby(string physicalDrive, string smartctl)
        {
            // Run(smartctl, $"-s standby,now {physicalDrive}");
            Run(smartctl, $"{physicalDrive} -Q");
        }
        public static void WakeUp(string physicalDrive, string smartctl)
        {
            // Run(smartctl, $"-s standby,now {physicalDrive}");
            Run(smartctl, $"-S");
        }

        private static string Run(string file, string args)
        {
            var p = new Process();
            p.StartInfo.FileName = file;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;

            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }
    }
}
