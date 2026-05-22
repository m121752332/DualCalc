using System;
using DualCalc.Services;
using Xunit;

namespace DualCalc.Tests
{
    public class ThemeServiceTests
    {
        // 模擬 MainWindow 的 ThemeButton_Click 邏輯
        private void SimulateThemeButtonClick(ThemeService service)
        {
            if (service.Theme == AppTheme.Light)
            {
                service.Theme = AppTheme.Dark;
            }
            else if (service.Theme == AppTheme.Dark)
            {
                service.Theme = AppTheme.Light;
            }
            else
            {
                // 如果是 System，切換到 Dark
                service.Theme = AppTheme.Dark;
            }
        }

        [Fact]
        public void ThemeButtonClick_WhenLight_ShouldChangeToDark()
        {
            // Arrange
            var themeService = ThemeService.Instance;
            themeService.Theme = AppTheme.Light;

            // Act
            SimulateThemeButtonClick(themeService);

            // Assert
            Assert.Equal(AppTheme.Dark, themeService.Theme);
            Assert.True(themeService.IsDark);
        }

        [Fact]
        public void ThemeButtonClick_WhenDark_ShouldChangeToLight()
        {
            // Arrange
            var themeService = ThemeService.Instance;
            themeService.Theme = AppTheme.Dark;

            // Act
            SimulateThemeButtonClick(themeService);

            // Assert
            Assert.Equal(AppTheme.Light, themeService.Theme);
            Assert.True(themeService.IsLight);
        }

        [Fact]
        public void ThemeButtonClick_WhenSystem_ShouldChangeToDark()
        {
            // Arrange
            var themeService = ThemeService.Instance;
            themeService.Theme = AppTheme.System;

            // Act
            SimulateThemeButtonClick(themeService);

            // Assert
            Assert.Equal(AppTheme.Dark, themeService.Theme);
            Assert.True(themeService.IsDark);
        }
    }
}
