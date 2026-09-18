using System;
using System.Net;

namespace NetControl.Throttling
{
    /// <summary>
    /// Cross-platform abstraction for per-device network bandwidth throttling.
    /// </summary>
    public interface IBandwidthThrottler : IDisposable
    {
        string InterfaceName { get; }

        /// <summary>
        /// Sets the maximum bandwidth limit for a target IP in KB/s.
        /// If limitKbps <= 0, the limit is removed.
        /// </summary>
        void SetLimit(IPAddress ip, int limitKbps);

        /// <summary>
        /// Removes any active bandwidth limit for the given target IP.
        /// </summary>
        void RemoveLimit(IPAddress ip);

        /// <summary>
        /// Resets and clears all active throttling rules on the interface.
        /// </summary>
        void ResetAll();
    }
}

