using DualCalc.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace DualCalc.Views
{
    public sealed partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel { get; } = new();

        public SettingsView()
        {
            this.InitializeComponent();

            ViewModel.Loc.LanguageChanged += (_, _) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Bindings.Update();
                });
            };
        }

        private void LangRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                if (tag == "ZhHant") ViewModel.IsZhHant = true;
                else if (tag == "ZhHans") ViewModel.IsZhHans = true;
            }
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                if (tag == "System") ViewModel.IsThemeSystem = true;
                else if (tag == "Light") ViewModel.IsThemeLight = true;
                else if (tag == "Dark") ViewModel.IsThemeDark = true;
            }
        }
    }
}
