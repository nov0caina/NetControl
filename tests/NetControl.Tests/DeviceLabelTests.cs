using System.Net.NetworkInformation;
using Xunit;

namespace NetControl.Tests
{
    public class DeviceLabelTests
    {
        [Fact]
        public void DeviceLabel_ShouldFormatMobileBrandAndHostname()
        {
            var pc = new PC
            {
                Hostname = "iPhone-de-Carlos",
                Vendor = "Apple",
                DeviceCategory = DeviceType.Mobile
            };

            Assert.Equal("Mobile — iPhone-de-Carlos (Apple)", pc.DeviceLabel);
        }

        [Fact]
        public void DeviceLabel_ShouldDeduplicateVendorWhenAlreadyInHostname()
        {
            var pc = new PC
            {
                Hostname = "Samsung-SmartTV",
                Vendor = "Samsung",
                DeviceCategory = DeviceType.SmartTV
            };

            Assert.Equal("SmartTV — Samsung-SmartTV", pc.DeviceLabel);
        }

        [Fact]
        public void DeviceLabel_ShouldFormatRandomizedMac()
        {
            // 74:D8:3E has locally administered bit set (byte 0 bit 1 = 0x02)
            var pc = new PC
            {
                Mac = PhysicalAddress.Parse("74D83E6E1BC6"),
                Vendor = "Randomized MAC"
            };

            Assert.Equal("Randomized 74:D8:3E:6E:1B:C6", pc.DeviceLabel);
        }

        [Fact]
        public void DeviceLabel_ShouldApplyEllipsisToExcessiveHostnames()
        {
            var pc = new PC
            {
                Hostname = "VeryLongHostnameThatExceedsNormalLengthLimitForNetworkDevicesTesting12345",
                DeviceCategory = DeviceType.Desktop
            };

            Assert.StartsWith("Desktop — VeryLongHostnameThatExceedsNormalLengthLimit", pc.DeviceLabel);
            Assert.EndsWith("…", pc.DeviceLabel);
        }
    }
}
