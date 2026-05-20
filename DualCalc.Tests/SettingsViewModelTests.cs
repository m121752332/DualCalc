using DualCalc.Services;
using DualCalc.ViewModels;
using Xunit;

namespace DualCalc.Tests
{
    public class SettingsViewModelTests
    {
        [Fact]
        public void Language_Change_Updates_Service()
        {
            var vm = new SettingsViewModel();
            var service = LocalizationService.Instance;

            vm.IsZhHans = true;
            Assert.Equal(AppLanguage.ZhHans, service.Language);

            vm.IsZhHant = true;
            Assert.Equal(AppLanguage.ZhHant, service.Language);
        }

        [Fact]
        public void Theme_Change_Updates_Service()
        {
            var vm = new SettingsViewModel();
            var service = ThemeService.Instance;

            vm.IsThemeLight = true;
            Assert.Equal(AppTheme.Light, service.Theme);

            vm.IsThemeDark = true;
            Assert.Equal(AppTheme.Dark, service.Theme);

            vm.IsThemeSystem = true;
            Assert.Equal(AppTheme.System, service.Theme);
        }
    }
}