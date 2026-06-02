using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SlimeDiskHandler
{
    public class DiskMonitor : IDisposable
    {
        private readonly List<DiskProfile> _profiles;
        private readonly CancellationTokenSource _cts = new();

        private Task? _loop;

        // 🟢 Tray / UI status cache
        public Dictionary<string, DiskStatus> StatusMap { get; } = new();

        public event Action? OnStatusUpdated;

        // 🛑 Anti-spin protection state
        private class DiskState
        {
            public DateTime LastStandby = DateTime.MinValue;
            public bool Sleeping = false;
        }

        private readonly Dictionary<string, DiskState> _state = new();

        private static readonly TimeSpan StandbyCooldown = TimeSpan.FromMinutes(15);

        public DiskMonitor()
        {
            _profiles = JsonSerializer.Deserialize<List<DiskProfile>>(
                File.ReadAllText("profiles.json"))!;
        }

        public void Start()
        {
            _loop = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    RunOnce();
                    await Task.Delay(TimeSpan.FromMinutes(1), _cts.Token);
                }
            });
        }

        public void RunOnce()
        {
            foreach (var profile in _profiles)
            {
                try
                {
                    //var physical = HDDHelper.GetPhysicalDrive(profile.DriveLetter);
                    var physical = profile.DriveLetter;
                    if (physical == null)
                        continue;

                    var temp = SmartHelper.GetTemp(
                        physical,
                        profile.SmartctlPath);

                    if (temp == null)
                        continue;

                    if ( profile.EnableLogging)
                    {
                        Log($"Drive {profile.DriveLetter} Temp={temp}°C");
                    }
                    bool idle = DiskIdleChecker.IsIdle(
                        physical,
                        1024,
                        profile.IdleTimeMinutes * 60);

                    // 🟢 STATUS UPDATE (tray tooltip)
                    StatusMap[profile.DriveLetter] = new DiskStatus
                    {
                        Drive = profile.DriveLetter,
                        Temp = temp,
                        Idle = idle
                    };

                    if (!_state.ContainsKey(profile.DriveLetter))
                        _state[profile.DriveLetter] = new DiskState();

                    var state = _state[profile.DriveLetter];

                    DateTime now = DateTime.Now;

                    // 🧠 HYSTERESIS (avoid spam wake/sleep)
                    bool isActive =
                        !idle || temp < profile.TempThreshold - 5;

                    if (isActive)
                    {
                        state.Sleeping = false;
                    }

                    bool cooldownActive =
                        now - state.LastStandby < StandbyCooldown;

                    bool shouldSleep =
                        idle && temp >= profile.TempThreshold;

                    // 🔴 ANTI-SPIN LOGIC
                    if ((shouldSleep &&
                        !cooldownActive &&
                        !state.Sleeping)||profile.PowerOffEvenWehnUsed)
                    {
                        SmartHelper.Standby(
                            physical,
                            profile.PowerOffCmd);

                        state.LastStandby = now;
                        state.Sleeping = true;
                        if (profile.EnableLogging)
                        {
                            Log($"Drive {profile.DriveLetter} Temp={temp}°C Powered Off");
                        }
                    }
                    else if (cooldownActive==false)
                    {
                        state.Sleeping = false;
                        //state.LastStandby = now;
                        if (profile.EnableLogging)
                        {
                            Log($"Searching for new HDDs");
                        }
                        SmartHelper.WakeUp(
                            physical,
                            profile.PowerOffCmd);
                    }
                }
                catch
                {
                    // ignore per-drive failures
                }
            }

            OnStatusUpdated?.Invoke();
        }
        public void Log(string message)
        {
             if ( Directory.Exists("logs") == false ) {
                Directory.CreateDirectory("logs");
            }
            File.AppendAllText(
                $"logs\\{DateTime.Now:dd-MM-yyyy}.txt",
                $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] " +
                $"{message}{Environment.NewLine}");
            
        }
        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
