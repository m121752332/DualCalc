using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DualCalc.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ── Two calculator instances ──────────────────────────
        public CalculatorViewModel CalcA { get; }
        public CalculatorViewModel CalcB { get; }

        public MainViewModel()
        {
            CalcA = new CalculatorViewModel(CalculatorIdentifier.Left) { MainViewModel = this };
            CalcB = new CalculatorViewModel(CalculatorIdentifier.Right) { MainViewModel = this };

            DualCalc.Services.LocalizationService.Instance.LanguageChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(DualModeTooltip));
            };
        }

        // ── Dual mode toggle ──────────────────────────────────
        private bool _isDualMode = DualCalc.Services.ConfigService.Instance.Calc.IsDualModeOnStartup;
        public bool IsDualMode
        {
            get => _isDualMode;
            set
            {
                if (_isDualMode == value) return;
                _isDualMode = value;

                // Update identifiers to refresh labels depending on dual mode state
                if (_isDualMode)
                {
                    CalcA.Identifier = CalculatorIdentifier.Left;
                    CalcB.Identifier = CalculatorIdentifier.Right;
                }
                else
                {
                    CalcA.Identifier = CalculatorIdentifier.Single;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(DualModeIcon));
                OnPropertyChanged(nameof(DualModeTooltip));
                OnPropertyChanged(nameof(DualModeToggleBackground));
            }
        }

        public string DualModeIcon    => _isDualMode ? "\uE923" : "\uE922"; // WinUI grid icons
        public string DualModeTooltip => _isDualMode 
            ? DualCalc.Services.LocalizationService.Instance.DualMode_Toggle_Dual 
            : DualCalc.Services.LocalizationService.Instance.DualMode_Toggle_Single; 

        // Button dynamic coloring based on dual mode 
        public Microsoft.UI.Xaml.Media.SolidColorBrush DualModeToggleBackground => _isDualMode 
            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent) // Default background for transparent 
            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.SeaGreen); 

        public void ToggleDualMode() => IsDualMode = !IsDualMode;

        // ── INotifyPropertyChanged ────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
