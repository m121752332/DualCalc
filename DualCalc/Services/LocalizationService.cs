using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Windows.ApplicationModel.Resources;
using Windows.ApplicationModel;

namespace DualCalc.Services
{
    public enum AppLanguage { ZhTw, ZhCn, EnUs, JaJp }

    /// <summary>
    /// 語言切換服務：繁體 / 簡體 / 英文 / 日文，即時連動 UI
    /// </summary>
    public class LocalizationService : INotifyPropertyChanged
    {
        // ── Singleton ─────────────────────────────────────────
        public static LocalizationService Instance { get; } = new();

        private Dictionary<string, string> _resources = new();

        private LocalizationService()
        {
            Load();

            // Apply config default language if not already saved in user local settings
            if (!File.Exists(_settingsFile))
            {
                var defaultLang = ConfigService.Instance.Setting.Language;
                if (string.Equals(defaultLang, "zh-CN", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(defaultLang, "zh-Hans", StringComparison.OrdinalIgnoreCase))
                {
                    _language = AppLanguage.ZhCn;
                }
                else if (string.Equals(defaultLang, "en-US", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(defaultLang, "en", StringComparison.OrdinalIgnoreCase))
                {
                    _language = AppLanguage.EnUs;
                }
                else if (string.Equals(defaultLang, "ja-JP", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(defaultLang, "ja", StringComparison.OrdinalIgnoreCase))
                {
                    _language = AppLanguage.JaJp;
                }
                else
                {
                    _language = AppLanguage.ZhTw;
                }
            }

            LoadStringsForCurrentLanguage();
        }

        // ── State ─────────────────────────────────────────────
        private AppLanguage _language = AppLanguage.ZhTw;
        public static string LastLoadError { get; private set; } = string.Empty;

        public AppLanguage Language
        {
            get => _language;
            set
            {
                if (_language == value) return;
                _language = value;
                LoadStringsForCurrentLanguage();
                Save();
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsZhTw));
                OnPropertyChanged(nameof(IsZhCn));
                OnPropertyChanged(nameof(IsEnUs));
                OnPropertyChanged(nameof(IsJaJp));
                NotifyAllStrings();
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsZhTw => _language == AppLanguage.ZhTw;
        public bool IsZhCn => _language == AppLanguage.ZhCn;
        public bool IsEnUs => _language == AppLanguage.EnUs;
        public bool IsJaJp => _language == AppLanguage.JaJp;

        public event EventHandler? LanguageChanged;

        private void LoadStringsForCurrentLanguage()
        {
            string fileName = _language switch
            {
                AppLanguage.ZhCn => "zh-CN.json",
                AppLanguage.EnUs => "en-US.json",
                AppLanguage.JaJp => "ja-JP.json",
                _ => "zh-TW.json"
            };

            string[] possiblePaths = new string[3];

            // 1. AppDomain/AppContext BaseDirectory
            var tempOrAppDomainPath = System.AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(tempOrAppDomainPath))
            {
                tempOrAppDomainPath = System.AppContext.BaseDirectory;
            }
            possiblePaths[0] = Path.Combine(tempOrAppDomainPath, "i18n", fileName);

            // 2. Process path directory + i18n
            var processDir = Path.GetDirectoryName(System.Environment.ProcessPath) ?? "";
            possiblePaths[1] = Path.Combine(processDir, "i18n", fileName);

            // 3. Fallback just in case
            possiblePaths[2] = Path.Combine(processDir, fileName);

            string? finalPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    finalPath = path;
                    break;
                }
            }

            if (finalPath != null)
            {
                try
                {
                    string json = File.ReadAllText(finalPath);
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (parsed != null)
                    {
                        _resources = parsed;
                    }
                    LastLoadError = string.Empty;
                }
                catch (System.Exception ex)
                {
                    LastLoadError = $"解析語系檔失敗:\n路徑: {finalPath}\n錯誤: {ex.Message}";
                }
            }
            else
            {
                LastLoadError = $"找不到語系檔，嘗試過以下路徑:\n1. {possiblePaths[0]}\n2. {possiblePaths[1]}\n3. {possiblePaths[2]}";
            }
        }

        // ── Resource lookup ───────────────────────────────────
        public string Get(string key)
        {
            return _resources.TryGetValue(key, out var value) ? value : key;
        }

        // ── Shorthand properties for common strings ────────────
        // These notify UI when language changes
        public string Nav_Calculator => Get("Nav_Calculator");
        public string Nav_Settings   => Get("Nav_Settings");
        public string Nav_About      => Get("Nav_About");
        public string Settings_Language     => Get("Settings_Language");
        public string Settings_ZhTw         => Get("Settings_ZhTw");
        public string Settings_ZhCn         => Get("Settings_ZhCn");
        public string Settings_EnUs         => Get("Settings_EnUs");
        public string Settings_JaJp         => Get("Settings_JaJp");
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
            OnPropertyChanged(nameof(Settings_EnUs));
            OnPropertyChanged(nameof(Settings_JaJp));
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
                    ? new List<string>(File.ReadAllLines(_settingsFile))
                    : new List<string>();

                string langStr = _language switch
                {
                    AppLanguage.ZhCn => "zh-CN",
                    AppLanguage.EnUs => "en-US",
                    AppLanguage.JaJp => "ja-JP",
                    _ => "zh-TW"
                };

                string entry = $"AppLanguage={langStr}";
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
                            if (string.Equals(stored, "zh-CN", StringComparison.OrdinalIgnoreCase) || string.Equals(stored, "zh-Hans", StringComparison.OrdinalIgnoreCase))
                                _language = AppLanguage.ZhCn;
                            else if (string.Equals(stored, "en-US", StringComparison.OrdinalIgnoreCase) || string.Equals(stored, "en", StringComparison.OrdinalIgnoreCase))
                                _language = AppLanguage.EnUs;
                            else if (string.Equals(stored, "ja-JP", StringComparison.OrdinalIgnoreCase) || string.Equals(stored, "ja", StringComparison.OrdinalIgnoreCase))
                                _language = AppLanguage.JaJp;
                            else
                                _language = AppLanguage.ZhTw;
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
