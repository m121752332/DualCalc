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

        // ── 綁定視窗第二個計算機欄位的寬度 ─────────────────────
        // 根據 IsDualMode 狀態來決定是否顯示第二個計算機區塊（1* 或 0）
        public GridLength DualColumnWidth =>
            ViewModel.IsDualMode ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // 初始化應用程式的預設寬高，並依據雙台模式設定目前的視窗最小寬度
            _baseMinWidth = ConfigService.Instance.Calc.DefaultAppWidth;
            _baseMinHeight = ConfigService.Instance.Calc.DefaultAppHeight;
            _currentMinWidth = ViewModel.IsDualMode ? _baseMinWidth * 2 : _baseMinWidth;
            _windowProc = WindowProc;

            // 註冊 Windows Hook 以控制視窗的最小尺寸限制
            InitializeMinimumWindowSize();

            // 將計算機元件的 ViewModel 進行綁定（確保兩個計算機能正常運作）
            CalcViewA.ViewModel = ViewModel.CalcA;   // re-bind after InitializeComponent
            CalcViewB.ViewModel = ViewModel.CalcB;

            // 初始化與套用主題服務
            ThemeService.Instance.Initialize((FrameworkElement)this.Content);

            // 訂閱主題變更事件，當主題改變時自動更新按鈕圖標
            ThemeService.Instance.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ThemeService.Theme))
                {
                    this.DispatcherQueue.TryEnqueue(UpdateThemeButtonIcon);
                }
            };

            // 監聽實際主題變化 (當設定為 System 時，系統切換時會觸發)
            if (this.Content is FrameworkElement rootElementInit)
            {
                rootElementInit.ActualThemeChanged += (_, _) =>
                {
                    if (ThemeService.Instance.Theme == AppTheme.System)
                    {
                        this.DispatcherQueue.TryEnqueue(UpdateThemeButtonIcon);
                    }
                };
            }

            // 初始化時立即同步按鈕圖標與當前主題
            UpdateThemeButtonIcon();

            // 當畫面因為某些原因載入時重新套用主題
            this.Activated += (s, e) => ThemeService.Instance.Apply();

            // 若設定檔載入有任何問題，當控制項載入完成後會顯示錯誤對話方塊
            var rootElement = this.Content as FrameworkElement;
            if (rootElement != null)
            {
                rootElement.Loaded += MainWindow_Loaded;
            }

            // 監聽 ViewModel 中 IsDualMode 屬性變化的事件，以動態更新視窗大小與畫面配置
            ViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsDualMode))
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        // 觸發重新綁定 UI 的第二個欄位寬度
                        OnPropertyChanged(nameof(DualColumnWidth));

                        // 判斷目前切換後的狀態來增加或縮小實體視窗的寬度
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

            // 監聽語言設定的變更，即時更新功能表（NavigationView）的文字
            Loc.LanguageChanged += (_, _) =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    Bindings.Update();

                    // 強制更新側邊導覽列文字
                    if (NavCalc.Content is TextBlock tbCalc) tbCalc.Text = Loc.Nav_Calculator;
                    if (NavSettings.Content is TextBlock tbSettings) tbSettings.Text = Loc.Nav_Settings;
                    if (NavAbout.Content is TextBlock tbAbout) tbAbout.Text = Loc.Nav_About;
                });
            };

            // 預設選擇載入計算機頁面
            NavView.SelectedItem = NavCalc;

            // 根據組態設定套用程式開啟時的視窗尺寸
            var appWidth = _baseMinWidth;
            var appHeight = _baseMinHeight;
            if (ViewModel.IsDualMode)
            {
                appWidth *= 2;
            }
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(appWidth, appHeight));
        }

        // ── 建立與實作限制視窗最小尺寸的 Windows Hook 邏輯 ──
        private void InitializeMinimumWindowSize()
        {
            _windowHandle = WindowNative.GetWindowHandle(this);
            _previousWndProc = SetWindowLongPtr(_windowHandle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_windowProc));
        }

        // 處理 Window Message 以攔截視窗大小改變並設定視窗下限
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

        // ── 畫面導覽邏輯（處理側邊欄點擊後切換頁面） ────────
        private void NavView_SelectionChanged(NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                // 先將所有頁面隱藏
                CalcPage.Visibility    = Visibility.Collapsed;
                SettingsPage.Visibility = Visibility.Collapsed;
                AboutPage.Visibility   = Visibility.Collapsed;

                // 根據使用者選取的項目，顯示對應的內容
                switch (item.Tag as string)
                {
                    case "Calculator": 
                        CalcPage.Visibility     = Visibility.Visible;
                        // 更新按鈕顏色，避免跨畫面造成的顏色顯示錯誤
                        ViewModel.NotifyDualModeToggleBackgroundChanged();
                        break;
                    case "Settings":   SettingsPage.Visibility = Visibility.Visible; break;
                    case "About":      AboutPage.Visibility    = Visibility.Visible; break;
                }
            }
        }

        // ── 當 MainWindow 第一個可視元件載入時的邏輯 ────────
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 若為設定檔讀取失敗，觸發視窗提示使用者
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

        // ── 提供 XAML 可直接綁定的 Boolean 轉 Visibility Helper 方法 ──
        public Visibility BoolToVisibility(bool value)
            => value ? Visibility.Visible : Visibility.Collapsed;

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var themeService = ThemeService.Instance;
            // Toggle theme: Light <-> Dark (如果是 System，則先切換到 Dark)
            if (themeService.Theme == AppTheme.Light)
            {
                themeService.Theme = AppTheme.Dark;
            }
            else if (themeService.Theme == AppTheme.Dark)
            {
                themeService.Theme = AppTheme.Light;
            }
            else
            {
                // System 預設切換到 Dark
                themeService.Theme = AppTheme.Dark;
            }
        }

        private void UpdateThemeButtonIcon()
        {
            var currentTheme = ThemeService.Instance.Theme;
            if (currentTheme == AppTheme.System)
            {
                var rootElement = this.Content as FrameworkElement;
                if (rootElement != null)
                {
                    // 根據實際顯示的主題來決定圖標
                    ThemeButton.Content = rootElement.ActualTheme == ElementTheme.Dark ? "\uE708" : "\uE706";
                }
                else
                {
                    ThemeButton.Content = "\uE706";
                }
            }
            else
            {
                // Dark theme shows moon icon (\uE708), otherwise sun icon (\uE706)
                ThemeButton.Content = currentTheme == AppTheme.Dark ? "\uE708" : "\uE706";
            }
        }

        // ── 提供屬性變化的事件支援（INotifyPropertyChanged） ──
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ── 定義 Win32 API 所需的結構與方法 ────────────────
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
