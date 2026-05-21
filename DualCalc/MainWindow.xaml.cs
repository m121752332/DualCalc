using DualCalc.Services;
using DualCalc.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using WinRT.Interop;

namespace DualCalc
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int WM_GETMINMAXINFO = 0x0024;
        private const int GWL_WNDPROC = -4;

        public MainViewModel ViewModel { get; } = new();
        public LocalizationService Loc => LocalizationService.Instance;

        private readonly int _baseMinWidth;
        private readonly int _baseMinHeight;
        private int _currentMinWidth;
        private readonly WndProc _windowProc;
        private IntPtr _windowHandle;
        private IntPtr _previousWndProc;

        // ── Dual column width binding ─────────────────────────
        public GridLength DualColumnWidth =>
            ViewModel.IsDualMode ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        public MainWindow()
        {
            this.InitializeComponent();

            _baseMinWidth = ConfigService.Instance.Calc.DefaultAppWidth;
            _baseMinHeight = ConfigService.Instance.Calc.DefaultAppHeight;
            _currentMinWidth = ViewModel.IsDualMode ? _baseMinWidth * 2 : _baseMinWidth;
            _windowProc = WindowProc;

            InitializeMinimumWindowSize();

            // Wire ViewModels to calculator views
            CalcViewA.ViewModel = ViewModel.CalcA;   // re-bind after InitializeComponent
            CalcViewB.ViewModel = ViewModel.CalcB;

            // Theme service init
            ThemeService.Instance.Initialize((FrameworkElement)this.Content);

            // Re-apply if it loaded before content was ready
            this.Activated += (s, e) => ThemeService.Instance.Apply();

            // Show config load error if any whenever Content is loaded
            var rootElement = this.Content as FrameworkElement;
            if (rootElement != null)
            {
                rootElement.Loaded += MainWindow_Loaded;
            }

            // Notify DualColumnWidth when IsDualMode changes
            ViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsDualMode))
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        // Trigger a re-measure by toggling binding
                        OnPropertyChanged(nameof(DualColumnWidth));

                        // Adjust window width when toggling dual mode
                        var appWidth = _baseMinWidth;
                        _currentMinWidth = ViewModel.IsDualMode ? appWidth * 2 : appWidth;
                        var currentSize = this.AppWindow.Size;
                        if (ViewModel.IsDualMode)
                        {
                            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(
                                Math.Max(appWidth * 2, currentSize.Width),
                                Math.Max(_baseMinHeight, currentSize.Height)));
                        }
                        else
                        {
                            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(
                                Math.Max(appWidth, currentSize.Width),
                                Math.Max(_baseMinHeight, currentSize.Height)));
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
            var appWidth = _baseMinWidth;
            var appHeight = _baseMinHeight;
            if (ViewModel.IsDualMode)
            {
                appWidth *= 2;
            }
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(appWidth, appHeight));
        }

        private void InitializeMinimumWindowSize()
        {
            _windowHandle = WindowNative.GetWindowHandle(this);
            _previousWndProc = SetWindowLongPtr(_windowHandle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_windowProc));
        }

        private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                var minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                minMaxInfo.ptMinTrackSize.x = _currentMinWidth;
                minMaxInfo.ptMinTrackSize.y = _baseMinHeight;
                Marshal.StructureToPtr(minMaxInfo, lParam, false);
            }

            return CallWindowProc(_previousWndProc, hWnd, msg, wParam, lParam);
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

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ConfigService.LastLoadError))
            {
                var dialog = new ContentDialog
                {
                    Title = "Config Load Error",
                    Content = ConfigService.LastLoadError,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();

                // Clear the error so it doesn't show again unless we reload
                ConfigService.LoadConfig(); // Try one more time, silently or handle accordingly, but here we just leave it so it doesn't loop
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

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
