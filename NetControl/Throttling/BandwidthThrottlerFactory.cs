using System;

namespace NetControl.Throttling
{
    /// <summary>
    /// Factory that selects and instantiates the optimal bandwidth throttling provider
    /// based on the runtime operating system (Linux, macOS, Windows/Managed).
    /// </summary>
    public static class BandwidthThrottlerFactory
    {
        public static IBandwidthThrottler Create(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                throw new ArgumentException("Interface name cannot be null or empty.", nameof(interfaceName));
            }

            if (OperatingSystem.IsLinux())
            {
                try
                {
                    return new LinuxTcThrottler(interfaceName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[THROTTLER] LinuxTcThrottler initialization failed: {ex.Message}. Falling back to TokenBucketThrottler.");
                    return new TokenBucketThrottler(interfaceName);
                }
            }

            if (OperatingSystem.IsMacOS())
            {
                try
                {
                    return new MacDummynetThrottler(interfaceName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[THROTTLER] MacDummynetThrottler initialization failed: {ex.Message}. Falling back to TokenBucketThrottler.");
                    return new TokenBucketThrottler(interfaceName);
                }
            }

            // Windows and other platforms use the resilient managed TokenBucketThrottler
            return new TokenBucketThrottler(interfaceName);
        }
    }
}

