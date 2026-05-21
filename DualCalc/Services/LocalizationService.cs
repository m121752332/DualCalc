using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DualCalc.Services
{
    public enum AppLanguage { ZhTw, ZhCn }

    /// <summary>
    /// 語言切換服務：繁體 / 簡體，即時連動 UI
    /// </summary>
    public class LocalizationService : INotifyPropertyChanged
    {
        // ── Singleton ─────────────────────────────────────────
        public static LocalizationService Instance { get; } = new();

        private static readonly IReadOnlyDictionary<string, string> ZhTwResources = new Dictionary<string, string>
        {
            ["Nav_Calculator"] = "計算機",
            ["Nav_Settings"] = "配置",
            ["Nav_About"] = "關於",
            ["Settings_Language"] = "語言",
            ["Settings_ZhTw"] = "繁體中文（zh-TW）",
            ["Settings_ZhCn"] = "簡體中文（zh-CN）",
            ["Settings_Theme"] = "介面主題",
            ["Settings_ThemeSystem"] = "系統",
            ["Settings_ThemeLight"] = "明亮",
            ["Settings_ThemeDark"] = "黑暗",
            ["About_Title"] = "DualCalc",
            ["About_Version"] = "版本 1.0.0",
            ["About_Frontend"] = "前端：WinUI 3 / Windows App SDK",
            ["About_Backend"] = "後端：C# / .NET 10",
            ["About_Copyright"] = "© 2026 DualCalc",
            ["DualMode_Toggle_Single"] = "單台計算機",
            ["DualMode_Toggle_Dual"] = "雙台計算機",
            ["Calc_Label_Left"] = "左邊計算機",
            ["Calc_Label_Right"] = "右邊計算機",
            ["Calc_Label_Single"] = "計算機",
            ["Calc_Memory_Clear"] = "記憶體清除",
            ["Calc_Memory_Recall"] = "記憶體讀取",
            ["Calc_Memory_Add"] = "記憶體加",
            ["Calc_Memory_Sub"] = "記憶體減",
            ["Calc_Memory_Store"] = "記憶體儲存",
            ["Error_DivZero"] = "除數不可為零",
            ["Error_Invalid"] = "無效的輸入"
        };

        private static readonly IReadOnlyDictionary<string, string> ZhCnResources = new Dictionary<string, string>
        {
            ["Nav_Calculator"] = "计算器",
            ["Nav_Settings"] = "配置",
            ["Nav_About"] = "关于",
            ["Settings_Language"] = "语言",
            ["Settings_ZhTw"] = "繁體中文（zh-TW）",
            ["Settings_ZhCn"] = "简体中文（zh-CN）",
            ["Settings_Theme"] = "界面主题",
            ["Settings_ThemeSystem"] = "系统",
            ["Settings_ThemeLight"] = "明亮",
            ["Settings_ThemeDark"] = "深色",
            ["About_Title"] = "DualCalc",
            ["About_Version"] = "版本 1.0.0",
            ["About_Frontend"] = "前端：WinUI 3 / Windows App SDK",
            ["About_Backend"] = "后端：C# / .NET 10",
            ["About_Copyright"] = "© 2026 DualCalc",
            ["DualMode_Toggle_Single"] = "单台计算器",
            ["DualMode_Toggle_Dual"] = "双台计算器",
            ["Calc_Label_Left"] = "左边计算器",
            ["Calc_Label_Right"] = "右边计算器",
            ["Calc_Label_Single"] = "计算器",
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

            // Apply config default language if not already saved in user local settings
            if (!File.Exists(_settingsFile))
            {
                var defaultLang = ConfigService.Instance.Setting.Language;
                if (string.Equals(defaultLang, "zh-CN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(defaultLang, "zh-Hans", StringComparison.OrdinalIgnoreCase))
                {
                    _language = AppLanguage.ZhCn;
                }
                else
                {
                    _language = AppLanguage.ZhTw;
                }
            }
        }

        // ── State ─────────────────────────────────────────────
        private AppLanguage _language = AppLanguage.ZhTw;
        public AppLanguage Language
        {
            get => _language;
            set
            {
                if (_language == value) return;
                _language = value;
                Save();
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsZhTw));
                OnPropertyChanged(nameof(IsZhCn));
                NotifyAllStrings();
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsZhTw => _language == AppLanguage.ZhTw;
        public bool IsZhCn => _language == AppLanguage.ZhCn;

        public event EventHandler? LanguageChanged;

        // ── Resource lookup ───────────────────────────────────
        public string Get(string key)
        {
            var resources = _language == AppLanguage.ZhTw ? ZhTwResources : ZhCnResources;
            return resources.TryGetValue(key, out var value) ? value : key;
        }

        // ── Shorthand properties for common strings ────────────
        // These notify UI when language changes
        public string Nav_Calculator => Get("Nav_Calculator");
        public string Nav_Settings   => Get("Nav_Settings");
        public string Nav_About      => Get("Nav_About");
        public string Settings_Language     => Get("Settings_Language");
        public string Settings_ZhTw         => Get("Settings_ZhTw");
        public string Settings_ZhCn         => Get("Settings_ZhCn");
        public string Settings_Theme        => Get("Settings_Theme");
        public string Settings_ThemeSystem  => Get("Settings_ThemeSystem");
        public string Settings_ThemeLight   => Get("Settings_ThemeLight");
        public string Settings_ThemeDark    => Get("Settings_ThemeDark");
        public string About_Title       => Get("About_Title");
        public string About_Version     => Get("About_Version");
        public string About_Frontend    => Get("About_Frontend");
        public string About_Backend     => Get("About_Backend");
        public string About_Copyright   => Get("About_Copyright");
        public string DualMode_Toggle_Single => Get("DualMode_Toggle_Single");
        public string DualMode_Toggle_Dual   => Get("DualMode_Toggle_Dual");
        public string Calc_Label_Left   => Get("Calc_Label_Left");
        public string Calc_Label_Right  => Get("Calc_Label_Right");
        public string Calc_Label_Single => Get("Calc_Label_Single");
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
            OnPropertyChanged(nameof(Settings_ZhTw));
            OnPropertyChanged(nameof(Settings_ZhCn));
            OnPropertyChanged(nameof(Settings_Theme));
            OnPropertyChanged(nameof(Settings_ThemeSystem));
            OnPropertyChanged(nameof(Settings_ThemeLight));
            OnPropertyChanged(nameof(Settings_ThemeDark));
            OnPropertyChanged(nameof(About_Title));
            OnPropertyChanged(nameof(About_Version));
            OnPropertyChanged(nameof(About_Frontend));
            OnPropertyChanged(nameof(About_Backend));
            OnPropertyChanged(nameof(About_Copyright));
            OnPropertyChanged(nameof(DualMode_Toggle_Single));
            OnPropertyChanged(nameof(DualMode_Toggle_Dual));
            OnPropertyChanged(nameof(Calc_Label_Left));
            OnPropertyChanged(nameof(Calc_Label_Right));
            OnPropertyChanged(nameof(Calc_Label_Single));
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

                string entry = $"AppLanguage={(_language == AppLanguage.ZhTw ? "zh-TW" : "zh-CN")}";
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
                            _language = string.Equals(stored, "zh-CN", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(stored, "zh-Hans", StringComparison.OrdinalIgnoreCase)
                                ? AppLanguage.ZhCn
                                : AppLanguage.ZhTw;
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
