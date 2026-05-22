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
        public bool IsZhTw
        {
            get => _loc.IsZhTw;
            set { if (value) _loc.Language = AppLanguage.ZhTw; OnPropertyChanged(); }
        }

        public bool IsZhCn
        {
            get => _loc.IsZhCn;
            set { if (value) _loc.Language = AppLanguage.ZhCn; OnPropertyChanged(); }
        }

        public bool IsEnUs
        {
            get => _loc.IsEnUs;
            set { if (value) _loc.Language = AppLanguage.EnUs; OnPropertyChanged(); }
        }

        public bool IsJaJp
        {
            get => _loc.IsJaJp;
            set { if (value) _loc.Language = AppLanguage.JaJp; OnPropertyChanged(); }
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

        public string LocalizationLoadError => LocalizationService.LastLoadError;
        public bool HasLocalizationError => !string.IsNullOrEmpty(LocalizationLoadError);

        public LocalizationService Loc => _loc;

        public SettingsViewModel()
        {
            _loc.LanguageChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsZhTw));
                OnPropertyChanged(nameof(IsZhCn));
                OnPropertyChanged(nameof(IsEnUs));
                OnPropertyChanged(nameof(IsJaJp));

                // Trigger reload for localization bound strings
                OnPropertyChanged(nameof(Loc));
                OnPropertyChanged(nameof(LocalizationLoadError));
                OnPropertyChanged(nameof(HasLocalizationError));
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
