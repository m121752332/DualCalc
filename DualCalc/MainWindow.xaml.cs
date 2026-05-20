using DualCalc.Services;
using DualCalc.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DualCalc
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainViewModel ViewModel { get; } = new();
        public LocalizationService Loc => LocalizationService.Instance;

        // ── Dual column width binding ─────────────────────────
        public GridLength DualColumnWidth =>
            ViewModel.IsDualMode ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        public MainWindow()
        {
            this.InitializeComponent();

            // Wire ViewModels to calculator views
            CalcViewA.ViewModel = ViewModel.CalcA;   // re-bind after InitializeComponent
            CalcViewB.ViewModel = ViewModel.CalcB;

            // Theme service init
            ThemeService.Instance.Initialize((FrameworkElement)this.Content);

            // Re-apply if it loaded before content was ready
            this.Activated += (s, e) => ThemeService.Instance.Apply();
            
            // Notify DualColumnWidth when IsDualMode changes
            ViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsDualMode))
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        // Trigger a re-measure by toggling binding
                        OnPropertyChanged(nameof(DualColumnWidth));

                        // Adjust window width when toggling dual mode
                        var appWidth = ConfigService.Instance.Calc.DefaultAppWidth;
                        var currentSize = this.AppWindow.Size;
                        if (ViewModel.IsDualMode)
                        {
                            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(appWidth * 2, currentSize.Height));
                        }
                        else
                        {
                            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(appWidth, currentSize.Height));
                        }
                    });
            };

            Loc.LanguageChanged += (_, _) =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    Bindings.Update();
                    
                    // Force NavigationView items to re-render
                    if (NavCalc.Content is TextBlock tbCalc) tbCalc.Text = Loc.Nav_Calculator;
                    if (NavSettings.Content is TextBlock tbSettings) tbSettings.Text = Loc.Nav_Settings;
                    if (NavAbout.Content is TextBlock tbAbout) tbAbout.Text = Loc.Nav_About;
                });
            };

            // Select Calculator nav item on load
            NavView.SelectedItem = NavCalc;

            // Window sizing based on config
            var appWidth = ConfigService.Instance.Calc.DefaultAppWidth;
            var appHeight = ConfigService.Instance.Calc.DefaultAppHeight;
            if (ViewModel.IsDualMode)
            {
                appWidth *= 2;
            }
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(appWidth, appHeight));
        }

        // ── Navigation ────────────────────────────────────────
        private void NavView_SelectionChanged(NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                CalcPage.Visibility    = Visibility.Collapsed;
                SettingsPage.Visibility = Visibility.Collapsed;
                AboutPage.Visibility   = Visibility.Collapsed;

                switch (item.Tag as string)
                {
                    case "Calculator": CalcPage.Visibility     = Visibility.Visible; break;
                    case "Settings":   SettingsPage.Visibility = Visibility.Visible; break;
                    case "About":      AboutPage.Visibility    = Visibility.Visible; break;
                }
            }
        }

        // ── Helper for x:Bind function binding ────────────────
        public Visibility BoolToVisibility(bool value)
            => value ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
