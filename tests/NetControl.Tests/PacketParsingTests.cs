using System;
using System.Net;
using System.Net.NetworkInformation;
using PacketDotNet;
using SharpPcap;
using Xunit;

namespace NetControl.Tests
{
    public class PacketParsingTests
    {
        [Fact]
        public void TryParseArpPacket_ShouldParseVlanTaggedArpFrame()
        {
            byte[] frame = BuildVlanArpFrame(
                PhysicalAddress.Parse("001122334455"),
                IPAddress.Parse("192.168.1.20"),
                PhysicalAddress.Parse("AABBCCDDEEFF"),
                IPAddress.Parse("192.168.1.1"));

            var capture = new RawCapture(LinkLayers.Ethernet, new PosixTimeval(0, 0), frame);

            bool parsed = CArp.TryParseArpPacket(
                capture,
                out IPAddress senderIp,
                out PhysicalAddress senderMac,
                out ArpOperation operation);

            Assert.True(parsed);
            Assert.Equal(IPAddress.Parse("192.168.1.20"), senderIp);
            Assert.Equal("001122334455", senderMac.ToString());
            Assert.Equal(ArpOperation.Response, operation);
        }

        [Fact]
        public void TryParseIpv4Packet_ShouldIgnoreVlanHeader()
        {
            byte[] frame = BuildVlanIpv4Frame(
                IPAddress.Parse("192.168.1.20"),
                IPAddress.Parse("8.8.8.8"));

            var capture = new RawCapture(LinkLayers.Ethernet, new PosixTimeval(0, 0), frame);

            bool parsed = CArp.TryParseIpv4Packet(
                capture,
                out IPAddress sourceIp,
                out IPAddress destinationIp);

            Assert.True(parsed);
            Assert.Equal(IPAddress.Parse("192.168.1.20"), sourceIp);
            Assert.Equal(IPAddress.Parse("8.8.8.8"), destinationIp);
        }

        [Fact]
        public void TryParseIpv4Packet_ShouldRejectTruncatedFrame()
        {
            var capture = new RawCapture(
                LinkLayers.Ethernet,
                new PosixTimeval(0, 0),
                new byte[] { 0, 1, 2, 3 });

            Assert.False(CArp.TryParseIpv4Packet(capture, out _, out _));
        }

        private static byte[] BuildVlanArpFrame(
            PhysicalAddress senderMac,
            IPAddress senderIp,
            PhysicalAddress targetMac,
            IPAddress targetIp)
        {
            byte[] frame = new byte[46];
            byte[] targetMacBytes = targetMac.GetAddressBytes();
            byte[] senderMacBytes = senderMac.GetAddressBytes();

            Array.Copy(targetMacBytes, 0, frame, 0, 6);
            Array.Copy(senderMacBytes, 0, frame, 6, 6);
            frame[12] = 0x81;
            frame[13] = 0x00;
            frame[14] = 0x00;
            frame[15] = 0x01;
            frame[16] = 0x08;
            frame[17] = 0x06;

            frame[18] = 0x00;
            frame[19] = 0x01;
            frame[20] = 0x08;
            frame[21] = 0x00;
            frame[22] = 0x06;
            frame[23] = 0x04;
            frame[24] = 0x00;
            frame[25] = 0x02;
            Array.Copy(senderMacBytes, 0, frame, 26, 6);
            Array.Copy(senderIp.GetAddressBytes(), 0, frame, 32, 4);
            Array.Copy(targetMacBytes, 0, frame, 36, 6);
            Array.Copy(targetIp.GetAddressBytes(), 0, frame, 42, 4);
            return frame;
        }

        private static byte[] BuildVlanIpv4Frame(IPAddress sourceIp, IPAddress destinationIp)
        {
            byte[] frame = new byte[38];
            frame[0] = 0xFF;
            frame[1] = 0xFF;
            frame[2] = 0xFF;
            frame[3] = 0xFF;
            frame[4] = 0xFF;
            frame[5] = 0xFF;
            frame[6] = 0x00;
            frame[7] = 0x11;
            frame[8] = 0x22;
            frame[9] = 0x33;
            frame[10] = 0x44;
            frame[11] = 0x55;
            frame[12] = 0x81;
            frame[13] = 0x00;
            frame[14] = 0x00;
            frame[15] = 0x01;
            frame[16] = 0x08;
            frame[17] = 0x00;
            frame[18] = 0x45;
            frame[19] = 0x00;
            frame[20] = 0x00;
            frame[21] = 0x14;
            frame[22] = 0x00;
            frame[23] = 0x00;
            frame[24] = 0x00;
            frame[25] = 0x00;
            frame[26] = 0x40;
            frame[27] = 0x11;
            frame[28] = 0x00;
            frame[29] = 0x00;
            Array.Copy(sourceIp.GetAddressBytes(), 0, frame, 30, 4);
            Array.Copy(destinationIp.GetAddressBytes(), 0, frame, 34, 4);
            return frame;
        }
    }
}
