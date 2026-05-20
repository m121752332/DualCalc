using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DualCalc.Services
{
    public enum AppLanguage { ZhHant, ZhHans }

    /// <summary>
    /// 語言切換服務：繁體 / 簡體，即時連動 UI
    /// </summary>
    public class LocalizationService : INotifyPropertyChanged
    {
        // ── Singleton ─────────────────────────────────────────
        public static LocalizationService Instance { get; } = new();

        private static readonly IReadOnlyDictionary<string, string> ZhHantResources = new Dictionary<string, string>
        {
            ["Nav_Calculator"] = "計算機",
            ["Nav_Settings"] = "配置",
            ["Nav_About"] = "關於",
            ["Settings_Language"] = "語言",
            ["Settings_ZhHant"] = "繁體中文",
            ["Settings_ZhHans"] = "简体中文",
            ["Settings_Theme"] = "介面主題",
            ["Settings_ThemeSystem"] = "系統",
            ["Settings_ThemeLight"] = "明亮",
            ["Settings_ThemeDark"] = "黑暗",
            ["About_Title"] = "DualCalc",
            ["About_Version"] = "版本 1.0.0",
            ["About_Frontend"] = "前端：WinUI 3 / Windows App SDK",
            ["About_Backend"] = "後端：C# / .NET 10",
            ["About_Copyright"] = "© 2026 DualCalc",
            ["DualMode_Toggle"] = "雙欄模式",
            ["Calc_Memory_Clear"] = "記憶體清除",
            ["Calc_Memory_Recall"] = "記憶體讀取",
            ["Calc_Memory_Add"] = "記憶體加",
            ["Calc_Memory_Sub"] = "記憶體減",
            ["Calc_Memory_Store"] = "記憶體儲存",
            ["Error_DivZero"] = "除數不可為零",
            ["Error_Invalid"] = "無效的輸入"
        };

        private static readonly IReadOnlyDictionary<string, string> ZhHansResources = new Dictionary<string, string>
        {
            ["Nav_Calculator"] = "计算器",
            ["Nav_Settings"] = "配置",
            ["Nav_About"] = "关于",
            ["Settings_Language"] = "语言",
            ["Settings_ZhHant"] = "繁體中文",
            ["Settings_ZhHans"] = "简体中文",
            ["Settings_Theme"] = "界面主题",
            ["Settings_ThemeSystem"] = "系统",
            ["Settings_ThemeLight"] = "明亮",
            ["Settings_ThemeDark"] = "深色",
            ["About_Title"] = "DualCalc",
            ["About_Version"] = "版本 1.0.0",
            ["About_Frontend"] = "前端：WinUI 3 / Windows App SDK",
            ["About_Backend"] = "后端：C# / .NET 10",
            ["About_Copyright"] = "© 2026 DualCalc",
            ["DualMode_Toggle"] = "双栏模式",
            ["Calc_Memory_Clear"] = "内存清除",
            ["Calc_Memory_Recall"] = "内存读取",
            ["Calc_Memory_Add"] = "内存加",
            ["Calc_Memory_Sub"] = "内存减",
            ["Calc_Memory_Store"] = "内存存储",
            ["Error_DivZero"] = "除数不可为零",
            ["Error_Invalid"] = "无效的输入"
        };

        private LocalizationService()
        {
            Load();
        }

        // ── State ─────────────────────────────────────────────
        private AppLanguage _language = AppLanguage.ZhHant;
        public AppLanguage Language
        {
            get => _language;
            set
            {
                if (_language == value) return;
                _language = value;
                Save();
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsZhHant));
                OnPropertyChanged(nameof(IsZhHans));
                NotifyAllStrings();
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsZhHant => _language == AppLanguage.ZhHant;
        public bool IsZhHans => _language == AppLanguage.ZhHans;

        public event EventHandler? LanguageChanged;

        // ── Resource lookup ───────────────────────────────────
        public string Get(string key)
        {
            var resources = _language == AppLanguage.ZhHant ? ZhHantResources : ZhHansResources;
            return resources.TryGetValue(key, out var value) ? value : key;
        }

        // ── Shorthand properties for common strings ────────────
        // These notify UI when language changes
        public string Nav_Calculator => Get("Nav_Calculator");
        public string Nav_Settings   => Get("Nav_Settings");
        public string Nav_About      => Get("Nav_About");
        public string Settings_Language     => Get("Settings_Language");
        public string Settings_ZhHant       => Get("Settings_ZhHant");
        public string Settings_ZhHans       => Get("Settings_ZhHans");
        public string Settings_Theme        => Get("Settings_Theme");
        public string Settings_ThemeSystem  => Get("Settings_ThemeSystem");
        public string Settings_ThemeLight   => Get("Settings_ThemeLight");
        public string Settings_ThemeDark    => Get("Settings_ThemeDark");
        public string About_Title       => Get("About_Title");
        public string About_Version     => Get("About_Version");
        public string About_Frontend    => Get("About_Frontend");
        public string About_Backend     => Get("About_Backend");
        public string About_Copyright   => Get("About_Copyright");
        public string DualMode_Toggle   => Get("DualMode_Toggle");
        public string Calc_Memory_Clear => Get("Calc_Memory_Clear");
        public string Calc_Memory_Recall => Get("Calc_Memory_Recall");
        public string Calc_Memory_Add   => Get("Calc_Memory_Add");
        public string Calc_Memory_Sub   => Get("Calc_Memory_Sub");
        public string Calc_Memory_Store => Get("Calc_Memory_Store");
        public string Error_DivZero     => Get("Error_DivZero");
        public string Error_Invalid     => Get("Error_Invalid");

        // ── Notify all string props when language changes ──────
        private void NotifyAllStrings()
        {
            OnPropertyChanged(nameof(Nav_Calculator));
            OnPropertyChanged(nameof(Nav_Settings));
            OnPropertyChanged(nameof(Nav_About));
            OnPropertyChanged(nameof(Settings_Language));
            OnPropertyChanged(nameof(Settings_ZhHant));
            OnPropertyChanged(nameof(Settings_ZhHans));
            OnPropertyChanged(nameof(Settings_Theme));
            OnPropertyChanged(nameof(Settings_ThemeSystem));
            OnPropertyChanged(nameof(Settings_ThemeLight));
            OnPropertyChanged(nameof(Settings_ThemeDark));
            OnPropertyChanged(nameof(About_Title));
            OnPropertyChanged(nameof(About_Version));
            OnPropertyChanged(nameof(About_Frontend));
            OnPropertyChanged(nameof(About_Backend));
            OnPropertyChanged(nameof(About_Copyright));
            OnPropertyChanged(nameof(DualMode_Toggle));
            OnPropertyChanged(nameof(Calc_Memory_Clear));
            OnPropertyChanged(nameof(Calc_Memory_Recall));
            OnPropertyChanged(nameof(Calc_Memory_Add));
            OnPropertyChanged(nameof(Calc_Memory_Sub));
            OnPropertyChanged(nameof(Calc_Memory_Store));
            OnPropertyChanged(nameof(Error_DivZero));
            OnPropertyChanged(nameof(Error_Invalid));
        }

        // ── Persistence ───────────────────────────────────────
        private static readonly string _settingsFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "DualCalc", "settings.txt");

        private void Save()
        {
            try
            {
                var lines = File.Exists(_settingsFile)
                    ? new System.Collections.Generic.List<string>(File.ReadAllLines(_settingsFile))
                    : new System.Collections.Generic.List<string>();

                string entry = $"AppLanguage={(_language == AppLanguage.ZhHant ? "zh-Hant" : "zh-Hans")}";
                int idx = lines.FindIndex(l => l.StartsWith("AppLanguage="));
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
                        if (line.StartsWith("AppLanguage="))
                        {
                            var stored = line["AppLanguage=".Length..].Trim();
                            _language = stored == "zh-Hans" ? AppLanguage.ZhHans : AppLanguage.ZhHant;
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
