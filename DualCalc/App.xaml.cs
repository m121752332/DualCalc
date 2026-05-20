using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace DualCalc
{
    public partial class App : Application
    {
        private Window? _window;
        private static readonly string StartupLogPath = Path.Combine(Path.GetTempPath(), "DualCalc.startup.log");

        public App()
        {
            Log("App ctor start");
            this.InitializeComponent();
            Log("App ctor after InitializeComponent");
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Log("OnLaunched start");
            _window = new MainWindow();
            Log("OnLaunched after MainWindow");
            _window.Activate();
            Log("OnLaunched after Activate");
        }

        internal static void Log(string message)
        {
            try
            {
                File.AppendAllText(StartupLogPath, $"{DateTime.Now:O} {message}{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }
}
