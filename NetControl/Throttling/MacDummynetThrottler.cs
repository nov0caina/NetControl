using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace NetControl.Throttling
{
    /// <summary>
    /// macOS native traffic shaper utilizing dummynet pipes (dnctl)
    /// and the BSD Packet Filter (pfctl) subsystem.
    /// </summary>
    public class MacDummynetThrottler : IBandwidthThrottler
    {
        public string InterfaceName { get; }

        private readonly ConcurrentDictionary<string, int> _activePipeIds = new();
        private int _nextPipeId = 100;
        private readonly object _lock = new();
        private bool _isDisposed = false;

        public MacDummynetThrottler(string interfaceName)
        {
            InterfaceName = interfaceName ?? throw new ArgumentNullException(nameof(interfaceName));
            ResetAll();
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
                int pipeId = _activePipeIds.GetOrAdd(ipStr, _ =>
                {
                    int id = _nextPipeId++;
                    return id;
                });

                // Configure dummynet pipe with bandwidth in KByte/s
                ExecuteCommand("dnctl", $"pipe {pipeId} config bw {limitKbps}KByte/s");
            }
        }

        public void RemoveLimit(IPAddress ip)
        {
            if (_isDisposed || ip == null) return;

            string ipStr = ip.ToString();
            lock (_lock)
            {
                if (_activePipeIds.TryRemove(ipStr, out int pipeId))
                {
                    ExecuteCommand("dnctl", $"pipe {pipeId} delete", ignoreErrors: true);
                }
            }
        }

        public void ResetAll()
        {
            lock (_lock)
            {
                _activePipeIds.Clear();
                ExecuteCommand("dnctl", "-q flush", ignoreErrors: true);
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

        private static bool ExecuteCommand(string binary, string arguments, bool ignoreErrors = false)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = binary,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null) return false;

                process.WaitForExit(1500);
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                {
                    Console.WriteLine($"[MAC THROTTLER WARN] Failed to execute '{binary} {arguments}': {ex.Message}");
                }
                return false;
            }
        }
    }
}

