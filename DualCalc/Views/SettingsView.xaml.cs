using DualCalc.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DualCalc.Views
{
    public sealed partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel { get; } = new();

        public SettingsView()
        {
            this.InitializeComponent();
        }
    }
}
