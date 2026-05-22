# DualCalc

Dual-column comparison calculator — WinUI 3 + C# + Windows App SDK

![Main UI](docs/app_main.png)
![Settings UI](docs/app_settings.png)

## Features

- **Dual-column mode**: Click the toggle on the toolbar to instantly expand/collapse the second calculator; the window size adjusts automatically based on settings, and color states with hints update dynamically.
- **Minimum main window size**: The main window now enforces minimum width and height limits so the calculator layout cannot be resized too small; single-column and dual-column modes use different minimum widths.
- **Configuration loading**: Added `config.yaml` support for default window size and startup mode (single/dual column), with corresponding logic in the settings page.
- **Operator precedence**: Uses the Shunting-yard algorithm with full operator-priority support.
- **Traditional/Simplified Chinese switching**: Switch instantly in Settings; all UI text (including dynamically updated hints) updates automatically.
- **Language code alignment**: Language settings were unified from `zh-Hant` / `zh-Hans` to `zh-TW` / `zh-CN`, while preserving compatibility when reading old saved values.
- **Three themes**: System / Light / Dark, applied immediately without restart.
- **Memory functions**: MC / MR / M+ / M− / MS.
- **About page logo**: `Assets/Logo.jpg` is now included in output and publish flow to avoid missing image issues at runtime.

---

## Requirements

| Tool | Version |
| ------ | ------ |
| Visual Studio | 2026 18.5+ |
| Windows App SDK | 2.0+ |
| .NET | 10.0 |
| Windows | 10 1809 (build 17763)+ |

### Visual Studio workloads

- **Windows application development**
- .NET desktop development

---

## Getting Started

```bash
# 1. Clone the project
git clone https://github.com/m121752332/DualCalc.git
cd DualCalc

# 2. Open with Visual Studio 2026
#    Open DualCalc.sln

# 3. Select x64 platform and run (F5)
```

---

## Project Structure

```xml
DualCalc/
├── Models/
│   └── CalculatorEngine.cs      # Shunting-yard calculation core
├── ViewModels/
│   ├── CalculatorViewModel.cs   # Single calculator state
│   ├── MainViewModel.cs         # Dual-column toggle logic
│   └── SettingsViewModel.cs     # Language + theme settings
├── Views/
│   ├── CalculatorView.xaml      # Calculator UI component
│   ├── SettingsView.xaml        # Settings page
│   └── AboutView.xaml           # About page
├── Services/
│   ├── ConfigService.cs         # config.yaml parameter loading service
│   ├── LocalizationService.cs   # Traditional/Simplified language service
│   └── ThemeService.cs          # Theme switching service
├── Converters/
│   └── BoolToVisibilityConverter.cs
├── Strings/
│   ├── zh-TW/Resources.resw     # Traditional Chinese resources
│   └── zh-CN/Resources.resw     # Simplified Chinese resources
└── config.yaml                  # Global app configuration file
```

---

## Additional Notes for This Update

### 1. Language and configuration adjustments

- The default language in `config.yaml` is now `zh-TW` / `zh-CN`.
- `SettingsViewModel`, `LocalizationService`, the Settings language options, and the `Strings` resource folder names were aligned to `zh-TW` / `zh-CN`.
- To avoid breaking upgraded users, the app still reads legacy saved values `zh-Hant` / `zh-Hans`, but future saves are written back as `zh-TW` / `zh-CN`.
- The language options shown on the Settings page are now:
	- `繁體中文（zh-TW）` in Traditional Chinese UI / `繁体中文（zh-TW）` in Simplified Chinese UI
  - `簡體中文（zh-CN）` in Traditional Chinese UI / `简体中文（zh-CN）` in Simplified Chinese UI

### 2. Packaging and publish notes

- `publish.ps1` was validated to produce standalone x64 and arm64 outputs correctly.
- The previous PRI default language warning was resolved by changing the project `DefaultLanguage` to `zh-TW`.
- Previous trimming warnings were removed by disabling `PublishTrimmed`, reducing runtime risk with WinUI / WinRT / YamlDotNet.
- `Assets/Logo.jpg` is now copied to output and publish folders so the About page image is available after publish.

### 3. Main window sizing behavior

- The main window can no longer be resized down to a layout-breaking size.
- The minimum height follows `defaultAppHeight` from `config.yaml`.
- The minimum width changes dynamically by mode:
  - Single-column mode: limited by the configured width for one calculator
  - Dual-column mode: limited by the configured width for two calculators

### 4. Notes

- If older README text still mentions `zh-Hant` / `zh-Hans`, the current implementation should be treated as `zh-TW` / `zh-CN`.
- The current visible language text is effectively driven by `LocalizationService` and its mapped resource keys.

---

## Tech Stack

| Layer | Technology |
| ---- | ------ |
| **Frontend** | WinUI 3 / Windows App SDK 2.0 |
| **Backend** | C# 12 / .NET 10 |
| **Architecture** | MVVM + x:Bind |
| **Calculation Engine** | Shunting-yard Algorithm |
| **Theme** | Mica Backdrop + ElementTheme |
| **Localization** | .resw ResourceLoader |
| **Config File** | YamlDotNet |

---

## Development Milestones

- [x] Phase 1 — Project architecture + CalculatorEngine + all ViewModel / Service components
- [ ] Phase 2 — UI detail refinements + animations
- [ ] Phase 3 — Keyboard input support
- [x] Phase 4 — Packaging release

---

## Packaging & Distribution

This project supports packaging the app into a clean standalone executable (all dependencies included, run directly with only the `.exe`) for both x64 and arm64 architectures.

Just run the prepared packaging script in Command Prompt or PowerShell:

```powershell
.\publish.ps1
```

After execution, `bin` / `obj` will be cleaned automatically, and packaged outputs will be generated at:
- **x64:** `DualCalc\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\`
- **arm64:** `DualCalc\bin\Release\net10.0-windows10.0.19041.0\win-arm64\publish\`

For distribution, place the generated `DualCalc.exe` and the root `config.yaml` in the same directory level so the app can load initial configuration correctly. If loading still fails, the app will show a popup describing the error reason.

---

© 2026 DualCalc
