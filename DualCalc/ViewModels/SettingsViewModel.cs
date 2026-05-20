using System.ComponentModel;
using System.Runtime.CompilerServices;
using DualCalc.Services;

namespace DualCalc.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly LocalizationService _loc = LocalizationService.Instance;
        private readonly ThemeService _theme = ThemeService.Instance;

        // ── Language ──────────────────────────────────────────
        public bool IsZhHant
        {
            get => _loc.IsZhHant;
            set { if (value) _loc.Language = AppLanguage.ZhHant; OnPropertyChanged(); }
        }

        public bool IsZhHans
        {
            get => _loc.IsZhHans;
            set { if (value) _loc.Language = AppLanguage.ZhHans; OnPropertyChanged(); }
        }

        // ── Theme ─────────────────────────────────────────────
        public bool IsThemeSystem
        {
            get => _theme.IsSystem;
            set { if (value) _theme.Theme = AppTheme.System; OnPropertyChanged(); }
        }

        public bool IsThemeLight
        {
            get => _theme.IsLight;
            set { if (value) _theme.Theme = AppTheme.Light; OnPropertyChanged(); }
        }

        public bool IsThemeDark
        {
            get => _theme.IsDark;
            set { if (value) _theme.Theme = AppTheme.Dark; OnPropertyChanged(); }
        }

        public LocalizationService Loc => _loc;

        public SettingsViewModel()
        {
            _loc.LanguageChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsZhHant));
                OnPropertyChanged(nameof(IsZhHans));
            };
            _theme.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ThemeService.IsSystem)
                    or nameof(ThemeService.IsLight)
                    or nameof(ThemeService.IsDark))
                {
                    OnPropertyChanged(nameof(IsThemeSystem));
                    OnPropertyChanged(nameof(IsThemeLight));
                    OnPropertyChanged(nameof(IsThemeDark));
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
