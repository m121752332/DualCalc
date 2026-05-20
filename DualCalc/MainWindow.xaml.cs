using DualCalc.Services;
using DualCalc.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DualCalc
{
    public sealed partial class MainWindow : Window
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

            // Notify DualColumnWidth when IsDualMode changes
            ViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsDualMode))
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        // Trigger a re-measure by toggling binding
                        OnPropertyChanged(nameof(DualColumnWidth));
                    });
            };

            // Select Calculator nav item on load
            NavView.SelectedItem = NavCalc;

            // Window sizing
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(380, 620));
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

        // ── INotifyPropertyChanged (for x:Bind) ──────────────
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
