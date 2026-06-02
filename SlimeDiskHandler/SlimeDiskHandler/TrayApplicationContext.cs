using System;
using System.Collections.Generic;
using System.Text;

namespace SlimeDiskHandler
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private readonly DiskMonitor _monitor;

        public TrayApplicationContext()
        {
            _monitor = new DiskMonitor();

            _tray = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "SlimeDiskHandler"
            };

            var menu = new ContextMenuStrip();

            menu.Items.Add("Force Check", null, (s, e) => _monitor.RunOnce());
            menu.Items.Add("Open Profiles", null, (s, e) =>
            {
                System.Diagnostics.Process.Start("notepad.exe",
                    "profiles.json");
            });

            menu.Items.Add("Exit", null, (s, e) =>
            {
                _tray.Visible = false;
                _monitor.Dispose();
                ExitThread();
            });

            _tray.ContextMenuStrip = menu;

            _monitor.Start();
        }
    }
}
