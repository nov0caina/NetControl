using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetControl
{
    /// <summary>
    /// Service that identifies network devices by resolving OUI vendor, hostname,
    /// and inferring device type through a layered strategy.
    /// </summary>
    public interface IDeviceIdentifierService : IDisposable
    {
        /// <summary>
        /// Identifies a device by resolving OUI vendor (instant), hostname (async DNS/mDNS),
        /// and inferring device type via heuristics. Updates PC properties reactively.
        /// </summary>
        Task IdentifyDeviceAsync(PC device, CancellationToken ct = default);

        /// <summary>
        /// Starts passive mDNS listener to capture device names from multicast traffic.
        /// </summary>
        void StartMdnsListener();

        /// <summary>
        /// Stops the mDNS listener.
        /// </summary>
        void StopMdnsListener();
    }
}
