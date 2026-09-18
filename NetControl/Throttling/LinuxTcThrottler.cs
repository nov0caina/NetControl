using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace NetControl.Throttling
{
    /// <summary>
    /// Linux native traffic shaper utilizing the kernel Traffic Control (tc)
    /// subsystem with Hierarchical Token Bucket (HTB) queue disciplines.
    /// </summary>
    public class LinuxTcThrottler : IBandwidthThrottler
    {
        public string InterfaceName { get; }

        private readonly ConcurrentDictionary<string, int> _activeClassIds = new();
        private int _nextClassId = 10;
        private readonly object _lock = new();
        private bool _isDisposed = false;

        public LinuxTcThrottler(string interfaceName)
        {
            InterfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            InitializeRootQdisc();
        }

        private void InitializeRootQdisc()
        {
            lock (_lock)
            {
                // Clear any existing root queue discipline on the interface
                ExecuteTc($"qdisc del dev {InterfaceName} root", ignoreErrors: true);

                // Add root HTB queue discipline with default class 9999 for unthrottled traffic
                ExecuteTc($"qdisc add dev {InterfaceName} root handle 1: htb default 9999");
                ExecuteTc($"class add dev {InterfaceName} parent 1: classid 1:9999 htb rate 1000mbit ceil 1000mbit");
            }
        }

        public void SetLimit(IPAddress ip, int limitKbps)
        {
            if (_isDisposed || ip == null) return;

            if (limitKbps <= 0)
            {
                RemoveLimit(ip);
                return;
            }

            string ipStr = ip.ToString();
            lock (_lock)
            {
                int classId = _activeClassIds.GetOrAdd(ipStr, _ =>
                {
                    int id = _nextClassId;
                    _nextClassId = (_nextClassId + 1) % 9000 + 10;
                    return id;
                });

                // Convert KB/s to kbit/s (1 KB/s = 8 kbit/s)
                int rateKbit = Math.Max(8, limitKbps * 8);

                // Create or replace HTB class with specified bandwidth rate and ceiling
                ExecuteTc($"class replace dev {InterfaceName} parent 1: classid 1:{classId} htb rate {rateKbit}kbit ceil {rateKbit}kbit");

                // Filter traffic directed to the victim IP
                ExecuteTc($"filter replace dev {InterfaceName} protocol ip parent 1: prio 1 handle {classId} u32 match ip dst {ipStr} flowid 1:{classId}");

                // Filter traffic coming from the victim IP
                ExecuteTc($"filter replace dev {InterfaceName} protocol ip parent 1: prio 1 handle {classId + 10000} u32 match ip src {ipStr} flowid 1:{classId}");
            }
        }

        public void RemoveLimit(IPAddress ip)
        {
            if (_isDisposed || ip == null) return;

            string ipStr = ip.ToString();
            lock (_lock)
            {
                if (_activeClassIds.TryRemove(ipStr, out int classId))
                {
                    ExecuteTc($"filter del dev {InterfaceName} protocol ip parent 1: prio 1 handle {classId} u32", ignoreErrors: true);
                    ExecuteTc($"filter del dev {InterfaceName} protocol ip parent 1: prio 1 handle {classId + 10000} u32", ignoreErrors: true);
                    ExecuteTc($"class del dev {InterfaceName} parent 1: classid 1:{classId}", ignoreErrors: true);
                }
            }
        }

        public void ResetAll()
        {
            lock (_lock)
            {
                _activeClassIds.Clear();
                ExecuteTc($"qdisc del dev {InterfaceName} root", ignoreErrors: true);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                ResetAll();
            }
            GC.SuppressFinalize(this);
        }

        private static bool ExecuteTc(string arguments, bool ignoreErrors = false)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "tc",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null) return false;

                process.WaitForExit(1500);
                if (process.ExitCode != 0 && !ignoreErrors)
                {
                    string err = process.StandardError.ReadToEnd().Trim();
                    Console.WriteLine($"[TC WARN] 'tc {arguments}' exited with code {process.ExitCode}: {err}");
                }
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                {
                    Console.WriteLine($"[TC ERROR] Failed to execute 'tc {arguments}': {ex.Message}");
                }
                return false;
            }
        }
    }
}

