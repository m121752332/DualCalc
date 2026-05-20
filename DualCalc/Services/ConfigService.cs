using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DualCalc.Services
{
    public class AppConfig
    {
        public CalcConfig Calc { get; set; } = new CalcConfig();
        public SettingConfig Setting { get; set; } = new SettingConfig();
    }

    public class CalcConfig
    {
        public int DefaultAppWidth { get; set; } = 380;
        public int DefaultAppHeight { get; set; } = 620;
        public bool IsDualModeOnStartup { get; set; } = true;
    }

    public class SettingConfig
    {
        public string Language { get; set; } = "zh-Hant";
        public string Theme { get; set; } = "System";
    }

    public static class ConfigService
    {
        private static AppConfig? _config;

        public static string LastLoadError { get; private set; } = string.Empty;

        public static AppConfig Instance
        {
            get
            {
                if (_config == null)
                {
                    LoadConfig();
                }
                return _config!;
            }
        }

        public static void LoadConfig()
        {
            string[] possiblePaths = new string[3];

            // 1. Check AppDomain base directory (might be a temp folder for single-file, or the actual dir for non-single-file)
            var tempOrAppDomainPath = System.AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(tempOrAppDomainPath))
            {
                tempOrAppDomainPath = System.AppContext.BaseDirectory;
            }
            possiblePaths[0] = Path.Combine(tempOrAppDomainPath, "config.yaml");

            // 2. To thoroughly handle single-file publish (.exe extraction), we use System.Environment.ProcessPath
            var processDir = Path.GetDirectoryName(System.Environment.ProcessPath) ?? "";

            // Expected path: ExeLocation\config\config.yaml
            possiblePaths[1] = Path.Combine(processDir, "config", "config.yaml");

            // Expected path: ExeLocation\config.yaml
            possiblePaths[2] = Path.Combine(processDir, "config.yaml");

            string? finalConfigPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    finalConfigPath = path;
                    break;
                }
            }

            if (finalConfigPath != null)
            {
                try
                {
                    var yaml = File.ReadAllText(finalConfigPath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance) // to match keys like defaultAppWidth
                        .Build();

                    _config = deserializer.Deserialize<AppConfig>(yaml);
                    LastLoadError = string.Empty; // Clear error on successful load
                }
                catch (System.Exception ex)
                {
                    LastLoadError = $"解析設定檔失敗:\n路徑: {finalConfigPath}\n錯誤: {ex.Message}";
                    _config = new AppConfig();
                }
            }
            else
            {
                LastLoadError = $"找不到設定檔，嘗試過以下路徑:\n1. {possiblePaths[0]}\n2. {possiblePaths[1]}\n3. {possiblePaths[2]}";
                _config = new AppConfig();
            }
        }
    }
}
