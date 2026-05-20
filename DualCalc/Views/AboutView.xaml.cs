using DualCalc.Services;
using Microsoft.UI.Xaml.Controls;

namespace DualCalc.Views
{
    public sealed partial class AboutView : Page
    {
        public LocalizationService Loc => LocalizationService.Instance;

        public AboutView()
        {
            this.InitializeComponent();

            Loc.LanguageChanged += (_, _) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Bindings.Update();
                });
            };
        }
    }
}
