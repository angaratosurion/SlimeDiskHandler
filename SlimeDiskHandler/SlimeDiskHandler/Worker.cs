using Microsoft.Extensions.Hosting;

namespace SlimeDiskHandler
{
    
    public class Worker : BackgroundService
    {
        private readonly List<DiskProfile> _profiles;
        private NotifyIcon? _tray;

        public Worker(List<DiskProfile> profiles)
        {
            _profiles = profiles;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _tray = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "SlimeDiskHandler"
            };

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var profile in _profiles)
                {
                    try
                    {
                        var physical =
                            HDDHelper.GetPhysicalDrive(
                                profile.DriveLetter);

                        if (physical == null)
                            continue;

                        var temp =
                            SmartHelper.GetTemp(
                                physical,
                                profile.SmartctlPath!);

                        if (temp == null)
                            continue;

                        bool idle =
                            DiskIdleChecker.IsIdle(
                                physical,
                                1024,
                                profile.IdleTimeMinutes * 60);

                        if (idle &&
                            temp >= profile.TempThreshold)
                        {
                            SmartHelper.Standby(
                                physical,
                                profile.PowerOffCmd);
                        }
                    }
                    catch
                    {
                    }
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }

        public override void Dispose()
        {
            _tray?.Dispose();
            base.Dispose();
        }
    }
}
