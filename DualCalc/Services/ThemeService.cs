using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;

namespace DualCalc.Services
{
    public enum AppTheme { System, Light, Dark }

    /// <summary>
    /// 主題切換服務：系統 / 明亮 / 黑暗，即時生效
    /// </summary>
    public class ThemeService : INotifyPropertyChanged
    {
        // ── Singleton ─────────────────────────────────────────
        public static ThemeService Instance { get; } = new();
        private ThemeService() { Load(); }

        // ── State ─────────────────────────────────────────────
        private AppTheme _theme = AppTheme.System;
        public AppTheme Theme
        {
            get => _theme;
            set
            {
                if (_theme == value) return;
                _theme = value;
                Apply();
                Save();
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSystem));
                OnPropertyChanged(nameof(IsLight));
                OnPropertyChanged(nameof(IsDark));
            }
        }

        public bool IsSystem => _theme == AppTheme.System;
        public bool IsLight  => _theme == AppTheme.Light;
        public bool IsDark   => _theme == AppTheme.Dark;

        // ── Apply ─────────────────────────────────────────────
        private FrameworkElement? _rootElement;

        public void Initialize(FrameworkElement rootElement)
        {
            _rootElement = rootElement;
            Apply();
        }

        public void Apply()
        {
            if (_rootElement == null) return;

            _rootElement.RequestedTheme = _theme switch
            {
                AppTheme.Light  => ElementTheme.Light,
                AppTheme.Dark   => ElementTheme.Dark,
                _               => ElementTheme.Default  // follows system
            };
        }

        // ── Persistence ───────────────────────────────────────
        private static readonly string _settingsFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "DualCalc", "settings.txt");

        private void Save()
        {
            try
            {
                // 讀取現有設定，更新 Theme 行，其他行保留
                var lines = File.Exists(_settingsFile)
                    ? new System.Collections.Generic.List<string>(File.ReadAllLines(_settingsFile))
                    : new System.Collections.Generic.List<string>();

                int idx = lines.FindIndex(l => l.StartsWith("AppTheme="));
                string entry = $"AppTheme={_theme}";
                if (idx >= 0) lines[idx] = entry;
                else lines.Add(entry);

                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFile)!);
                File.WriteAllLines(_settingsFile, lines);
            }
            catch { /* ignore write failures */ }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    foreach (var line in File.ReadAllLines(_settingsFile))
                    {
                        if (line.StartsWith("AppTheme="))
                        {
                            _theme = line["AppTheme=".Length..] switch
                            {
                                "Light" => AppTheme.Light,
                                "Dark"  => AppTheme.Dark,
                                _       => AppTheme.System
                            };
                        }
                    }
                }
            }
            catch { /* ignore read failures, use default */ }
        }

        // ── INotifyPropertyChanged ────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
