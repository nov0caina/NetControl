using System;
using System.Net.NetworkInformation;
using Xunit;

namespace SelfishNet.Tests
{
    public class OuiAndIdentificationTests
    {
        [Fact]
        public void OuiLookup_ShouldResolveIntelPrefix()
        {
            byte[] intelMacBytes = new byte[] { 0xBC, 0xF1, 0x71, 0xA2, 0x61, 0x2E };
            string vendor = OuiDatabase.Lookup(intelMacBytes);
            Assert.Equal("Intel", vendor);

            var intelDevice = new PC
            {
                Mac = PhysicalAddress.Parse("BCF171A2612E"),
                Vendor = vendor,
                DeviceCategory = DeviceType.Desktop
            };
            Assert.Equal("Desktop (Intel)", intelDevice.DeviceLabel);
        }

        [Theory]
        [InlineData("00:E0:4C:11:22:33", "Realtek")]
        [InlineData("00:03:93:AA:BB:CC", "Apple")]
        [InlineData("24:0A:C4:01:02:03", "Espressif")]
        [InlineData("00:0C:E7:55:66:77", "MediaTek")]
        [InlineData("00:15:99:12:34:56", "Samsung")]
        [InlineData("F4:F5:E8:99:88:77", "Google")]
        public void OuiLookup_ShouldResolveCommonVendors(string macStr, string expectedVendor)
        {
            var mac = PhysicalAddress.Parse(macStr.Replace(":", ""));
            string vendor = OuiDatabase.Lookup(mac.GetAddressBytes());
            Assert.Equal(expectedVendor, vendor);
        }

        [Fact]
        public void DeviceLabel_ShouldSanitizeInAddrArpaReverseDnsOnLocalHost()
        {
            var localPc = new PC
            {
                IsLocalPc = true,
                Hostname = "1.1.168.192.in-addr.arpa",
                Name = "MyLinuxBox"
            };

            Assert.Equal($"{Environment.MachineName} (Local Host)", localPc.DeviceLabel);
        }

        [Fact]
        public void DeviceLabel_ShouldPreserveValidLocalHostHostnames()
        {
            var localPcValid = new PC
            {
                IsLocalPc = true,
                Hostname = "workstation-pro",
                Name = "workstation-pro"
            };

            Assert.Equal("workstation-pro (Local Host)", localPcValid.DeviceLabel);
        }

        [Fact]
        public void GoogleCast_ShouldBeInferredAsSmartTvAndFormatted()
        {
            var castDevice = new PC
            {
                Hostname = "Chromecast / Google Cast",
                DeviceCategory = DeviceType.SmartTV
            };

            Assert.Equal(DeviceType.SmartTV, DeviceIdentifierService.InferDeviceType(new PC { Hostname = "_googlecast" }));
            Assert.Equal("SmartTV — Chromecast / Google Cast", castDevice.DeviceLabel);
        }
    }
}
