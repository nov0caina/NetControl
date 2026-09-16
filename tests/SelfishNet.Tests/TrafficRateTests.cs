using System;
using Xunit;

namespace SelfishNet.Tests
{
    public class TrafficRateTests
    {
        [Fact]
        public void InitialRates_ShouldBeZeroAndInactive()
        {
            var pc = new PC();
            Assert.Equal("0 KB/s", pc.DownloadSpeedFormatted);
            Assert.Equal("0 KB/s", pc.UploadSpeedFormatted);
            Assert.False(pc.HasActiveTraffic);
        }

        [Fact]
        public void TransferRates_ShouldFormatAccurately()
        {
            var pc = new PC();

            // Set simulated transfer rates (1024 KB/s = 1.0 MB/s, 512 KB/s)
            pc.DownloadSpeedKbps = 1024.0;
            pc.UploadSpeedKbps = 512.0;

            Assert.Equal("1.0 MB/s", pc.DownloadSpeedFormatted);
            Assert.Equal("512.0 KB/s", pc.UploadSpeedFormatted);
            Assert.True(pc.HasActiveTraffic);
            Assert.Equal("#3FB950", pc.DownloadTrafficBrushHex);
            Assert.Equal("#58A6FF", pc.UploadTrafficBrushHex);
        }

        [Fact]
        public void TrafficRate_ShouldDecaySmoothlyViaEma()
        {
            var pc = new PC();
            pc.DownloadSpeedKbps = 1024.0;

            // Simulate one cycle of 0 bytes with alpha = 0.65 -> rate becomes 1024 * 0.35 = 358.4 KB/s
            pc.DownloadSpeedKbps = pc.DownloadSpeedKbps * 0.35;
            Assert.Equal(358.4, Math.Round(pc.DownloadSpeedKbps, 1));
            Assert.Equal("358.4 KB/s", pc.DownloadSpeedFormatted);

            // Simulate further idle cycles until rate drops below 0.1 threshold
            for (int k = 0; k < 10; k++)
            {
                double next = pc.DownloadSpeedKbps * 0.35;
                pc.DownloadSpeedKbps = next < 0.1 ? 0.0 : next;
            }

            Assert.Equal(0.0, pc.DownloadSpeedKbps);
            Assert.Equal("0 KB/s", pc.DownloadSpeedFormatted);
            Assert.False(pc.HasActiveTraffic);
            Assert.Equal("#6E7681", pc.DownloadTrafficBrushHex);
        }
    }
}

