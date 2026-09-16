using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Xunit;

namespace SelfishNet.Tests
{
    public class AtomicAccumulatorTests
    {
        [Fact]
        public void AtomicAccumulators_ShouldAccumulateBytesCorrectlyUnderConcurrency()
        {
            var pc = new PC
            {
                Ip = IPAddress.Parse("192.168.1.100"),
                Mac = PhysicalAddress.Parse("001122334455")
            };

            // Run 10 parallel threads adding 1000 packets of 1500 bytes each
            Parallel.For(0, 10, i =>
            {
                for (int j = 0; j < 1000; j++)
                {
                    pc.AddBytesReceived(1500);
                    pc.AddBytesSent(1500);
                }
            });

            Assert.Equal(15_000_000, pc.BytesReceived);
            Assert.Equal(15_000_000, pc.BytesSent);

            var (down, up) = pc.ResetByteAccumulators();
            Assert.Equal(15_000_000, down);
            Assert.Equal(15_000_000, up);
            Assert.Equal(0, pc.BytesReceived);
            Assert.Equal(0, pc.BytesSent);
        }
    }
}

