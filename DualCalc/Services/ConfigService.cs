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
        public bool IsDualModeOnStartup { get; set; } = false;
    }

    public class SettingConfig
    {
        public string Language { get; set; } = "zh-Hant";
        public string Theme { get; set; } = "System";
    }

    public static class ConfigService
    {
        private static AppConfig? _config;

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

        private static void LoadConfig()
        {
            var configPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.yaml");

            if (File.Exists(configPath))
            {
                var yaml = File.ReadAllText(configPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance) // to match keys like defaultAppWidth
                    .Build();

                _config = deserializer.Deserialize<AppConfig>(yaml);
            }
            else
            {
                _config = new AppConfig();
            }
        }
    }
}
