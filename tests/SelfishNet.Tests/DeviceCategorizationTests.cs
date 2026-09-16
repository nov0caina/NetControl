using Xunit;

namespace SelfishNet.Tests
{
    public class DeviceCategorizationTests
    {
        [Fact]
        public void InferDeviceType_ShouldIdentifySmartTvs()
        {
            var lg = new PC { Hostname = "LGwebOSTV-Room", Vendor = "LG Electronics" };
            Assert.Equal(DeviceType.SmartTV, DeviceIdentifierService.InferDeviceType(lg));

            var roku = new PC { Hostname = "RokuUltra-4K", Vendor = "Roku" };
            Assert.Equal(DeviceType.SmartTV, DeviceIdentifierService.InferDeviceType(roku));

            var cast = new PC { Hostname = "_googlecast" };
            Assert.Equal(DeviceType.SmartTV, DeviceIdentifierService.InferDeviceType(cast));
        }

        [Fact]
        public void InferDeviceType_ShouldIdentifyConsoles()
        {
            var ps5 = new PC { Hostname = "PS5-LivingRoom", Vendor = "Sony Interactive Entertainment" };
            Assert.Equal(DeviceType.Console, DeviceIdentifierService.InferDeviceType(ps5));

            var nintendo = new PC { Hostname = "NintendoSwitch-Family", Vendor = "Nintendo" };
            Assert.Equal(DeviceType.Console, DeviceIdentifierService.InferDeviceType(nintendo));
        }

        [Fact]
        public void InferDeviceType_ShouldIdentifyMobilesAndTablets()
        {
            var pixel = new PC { Hostname = "Pixel-8-Pro", Vendor = "Google" };
            Assert.Equal(DeviceType.Mobile, DeviceIdentifierService.InferDeviceType(pixel));

            var ipad = new PC { Hostname = "iPad-Air", Vendor = "Apple" };
            Assert.Equal(DeviceType.Tablet, DeviceIdentifierService.InferDeviceType(ipad));
        }

        [Fact]
        public void InferDeviceType_ShouldIdentifyDesktops()
        {
            var mac = new PC { Hostname = "MacBook-Pro-M2", Vendor = "Apple" };
            Assert.Equal(DeviceType.Desktop, DeviceIdentifierService.InferDeviceType(mac));

            var win = new PC { Hostname = "DESKTOP-ABC1234", Vendor = "ASUSTeK COMPUTER INC." };
            Assert.Equal(DeviceType.Desktop, DeviceIdentifierService.InferDeviceType(win));
        }

        [Fact]
        public void InferDeviceType_ShouldIdentifyPrinters()
        {
            var hp = new PC { Hostname = "HP-LaserJet-M404", Vendor = "HP Inc." };
            Assert.Equal(DeviceType.Printer, DeviceIdentifierService.InferDeviceType(hp));
        }

        [Fact]
        public void InferDeviceType_ShouldIdentifyRouters()
        {
            var router = new PC { IsGateway = true, Hostname = "Gateway-AP" };
            Assert.Equal(DeviceType.Router, DeviceIdentifierService.InferDeviceType(router));
        }

        [Fact]
        public void InferDeviceType_ShouldIdentifyIoT()
        {
            var sonos = new PC { Hostname = "Sonos-One", Vendor = "Sonos" };
            Assert.Equal(DeviceType.IoT, DeviceIdentifierService.InferDeviceType(sonos));

            var esp = new PC { Hostname = "esp32-sensor-temp", Vendor = "Espressif" };
            Assert.Equal(DeviceType.IoT, DeviceIdentifierService.InferDeviceType(esp));
        }
    }
}

