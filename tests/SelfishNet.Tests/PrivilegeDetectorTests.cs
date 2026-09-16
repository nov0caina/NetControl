using Xunit;

namespace SelfishNet.Tests
{
    public class PrivilegeDetectorTests
    {
        [Fact]
        public void GetElevationGuidance_ShouldReturnPlatformGuidance()
        {
            string guidance = PrivilegeDetector.GetElevationGuidance();
            Assert.False(string.IsNullOrWhiteSpace(guidance));
            Assert.True(
                guidance.Contains("pkexec") ||
                guidance.Contains("administrator") ||
                guidance.Contains("sudo"),
                "Guidance must contain platform elevation instructions"
            );
        }

        [Fact]
        public void IsElevated_ShouldExecuteWithoutThrowingExceptions()
        {
            // Verifies that native P/Invoke or WindowsPrincipal checks execute safely across platforms
            var exception = Record.Exception(() =>
            {
                bool isElevated = PrivilegeDetector.IsElevated();
                _ = isElevated;
            });

            Assert.Null(exception);
        }

        [Fact]
        public void HasCaptureCapabilities_ShouldExecuteSafely()
        {
            var exception = Record.Exception(() =>
            {
                bool hasCaps = PrivilegeDetector.HasCaptureCapabilities();
                _ = hasCaps;
            });

            Assert.Null(exception);
        }
    }
}

