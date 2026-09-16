using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using SharpPcap;

namespace SelfishNet
{
    public static class PrivilegeDetector
    {
        [DllImport("libc", EntryPoint = "geteuid", SetLastError = true)]
        private static extern uint GetPosixEffectiveUserId();

        /// <summary>
        /// Determines whether the current process is running with root or administrative privileges.
        /// </summary>
        public static bool IsElevated()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return GetPosixEffectiveUserId() == 0;
                }
            }
            catch
            {
                // Graceful fallback if runtime P/Invoke or security inspection fails
            }

            return false;
        }

        /// <summary>
        /// Verifies whether the process either has superuser privileges or packet capture capabilities (e.g. cap_net_raw).
        /// </summary>
        public static bool HasCaptureCapabilities()
        {
            if (IsElevated())
            {
                return true;
            }

            try
            {
                var devices = CaptureDeviceList.Instance;
                return devices != null && devices.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns contextual instructions guiding the user on how to elevate permissions on the current OS.
        /// </summary>
        public static string GetElevationGuidance()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Please restart SelfishNet with 'Run as administrator' to enable packet capture and ARP redirection.";
            }

            return "Launch with 'pkexec' or 'sudo -E ./SelfishNet', or grant raw socket capabilities via 'sudo setcap cap_net_raw,cap_net_admin=eip <binary>'.";
        }
    }
}

