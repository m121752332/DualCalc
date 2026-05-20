using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DualCalc.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ── Two calculator instances ──────────────────────────
        public CalculatorViewModel CalcA { get; } = new("A");
        public CalculatorViewModel CalcB { get; } = new("B");

        public MainViewModel()
        {
            DualCalc.Services.LocalizationService.Instance.LanguageChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(DualModeTooltip));
            };
        }

        // ── Dual mode toggle ──────────────────────────────────
        private bool _isDualMode = true;
        public bool IsDualMode
        {
            get => _isDualMode;
            set
            {
                if (_isDualMode == value) return;
                _isDualMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DualModeIcon));
                OnPropertyChanged(nameof(DualModeTooltip));
            }
        }

        public string DualModeIcon    => _isDualMode ? "\uE923" : "\uE922"; // WinUI grid icons
        public string DualModeTooltip => _isDualMode ? DualCalc.Services.LocalizationService.Instance.DualMode_Toggle : DualCalc.Services.LocalizationService.Instance.DualMode_Toggle; 

        public void ToggleDualMode() => IsDualMode = !IsDualMode;

        // ── INotifyPropertyChanged ────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
