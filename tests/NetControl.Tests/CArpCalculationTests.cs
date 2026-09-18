using System.Net;
using Xunit;

namespace NetControl.Tests
{
    public class CArpCalculationTests
    {
        [Fact]
        public void GetNetworkAddress_ShouldCalculateCorrectSubnetBase()
        {
            byte[] ip = new byte[] { 192, 168, 1, 150 };
            byte[] mask = new byte[] { 255, 255, 255, 0 };

            byte[] network = CArp.GetNetworkAddress(ip, mask);
            Assert.Equal(new byte[] { 192, 168, 1, 0 }, network);
        }

        [Fact]
        public void GetBroadcastAddress_ShouldCalculateCorrectBroadcast()
        {
            byte[] ip = new byte[] { 192, 168, 1, 150 };
            byte[] mask = new byte[] { 255, 255, 255, 0 };

            byte[] bcast = CArp.GetBroadcastAddress(ip, mask);
            Assert.Equal(new byte[] { 192, 168, 1, 255 }, bcast);
        }

        [Fact]
        public void SubnetMask_Slash23_ShouldCalculateCorrectBounds()
        {
            byte[] ip = new byte[] { 10, 0, 1, 45 };
            byte[] mask = new byte[] { 255, 255, 254, 0 }; // /23

            byte[] network = CArp.GetNetworkAddress(ip, mask);
            byte[] bcast = CArp.GetBroadcastAddress(ip, mask);

            Assert.Equal(new byte[] { 10, 0, 0, 0 }, network);
            Assert.Equal(new byte[] { 10, 0, 1, 255 }, bcast);
        }
    }
}

