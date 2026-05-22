# DualCalc

> 此文件提供本專案中所需的完整背景知識。
> 放置於專案根目錄，每次進入時會自動讀取。

---

## 專案簡介

**DualCalc** 是一個 Windows 桌面計算機應用程式，主打「雙欄對照」功能，
讓使用者可同時操作兩個獨立計算機，比對不同算式是否得出相同結果。

---

## 技術棧

| 層 | 技術 | 版本 |
|----|------|------|
| 前端 | WinUI 3 / XAML | Windows App SDK 2.0 |
| 後端 | C# | .NET 10 |
| 最低系統 | Windows 10 | Build 17763 (1809) |
| IDE | Visual Studio | 2026 18.5+ |

---

## 專案結構

```xml
DualCalc/
├── Models/
│   └── CalculatorEngine.cs      # Shunting-yard 計算核心
├── ViewModels/
│   ├── CalculatorViewModel.cs   # 單一計算機狀態
│   ├── MainViewModel.cs         # 雙欄切換邏輯
│   └── SettingsViewModel.cs     # 語言 + 主題設定
├── Views/
│   ├── CalculatorView.xaml      # 計算機 UI 元件
│   ├── SettingsView.xaml        # 配置頁
│   └── AboutView.xaml           # 關於頁
├── Services/
│   ├── ConfigService.cs         # config.yaml 參數讀取服務
│   ├── LocalizationService.cs   # 繁簡切換服務
│   └── ThemeService.cs          # 主題切換服務
├── Converters/
│   └── BoolToVisibilityConverter.cs
├── i18n/
│   ├── en-US.json               # 英文語系字典
│   ├── ja-JP.json               # 日文語系字典
│   ├── zh-CN.json               # 簡體中文語系字典
│   └── zh-TW.json               # 繁體中文語系字典
└── config.yaml                  # 系統全域應用配置檔
```

---

## 架構原則

### MVVM
- **Model**：`CalculatorEngine`（純邏輯，無 UI 依賴）
- **ViewModel**：持有狀態，實作 `INotifyPropertyChanged`，供 `x:Bind` 使用
- **View**：XAML 只負責 UI，邏輯一律在 ViewModel 處理
- 禁止在 code-behind（`.xaml.cs`）裡寫業務邏輯，只放事件轉發

### Singleton Services
- `LocalizationService.Instance` — 全域語言狀態
- `ThemeService.Instance` — 全域主題狀態
- 兩者都持久化到 `ApplicationData.Current.LocalSettings`

### x:Bind 優先
- 一律使用 `x:Bind`（編譯期綁定），不用 `Binding`（執行期反射）
- 雙向綁定用 `Mode=TwoWay`，動態資料用 `Mode=OneWay`

---

## 關鍵設計決策

### 計算引擎：Shunting-yard Algorithm
- 檔案：`Models/CalculatorEngine.cs`
- 支援先乘除後加減（`×` `÷` 優先級 > `+` `-`）
- Token 化 → 轉 RPN（逆波蘭式）→ Stack 求值
- 運算子符號：`+` `-` `×` `÷`（注意：乘除用全形符號，不是 `*` `/`）
- 公開 API：`Evaluate(string)`, `Reciprocal`, `Square`, `SquareRoot`, `Negate`, `Format`

### 雙欄模式
- `MainViewModel.IsDualMode`（bool）控制右欄顯示
- `MainWindow.xaml` 用 `BoolToVisibilityConverter` 綁定 Calculator B 的 `Visibility`
- `DualColumnWidth` property 控制 Grid ColumnDefinition 寬度（0 或 `*`）
- Toolbar 上的 `A | B` ToggleButton 綁定 `IsDualMode`

### 語言切換（即時連動）
- 切換 `LocalizationService.Language` → 觸發 `LanguageChanged` 事件
- 事件內 `NotifyAllStrings()` 批次觸發所有字串 property 的 `PropertyChanged`
- XAML 所有文字透過 `{x:Bind Loc.Nav_Calculator, Mode=OneWay}` 自動重繪

### 主題切換
- `ThemeService.Initialize(rootElement)` 在 `MainWindow` 建構子呼叫
- 切換時呼叫 `rootElement.RequestedTheme`，即時生效，無需重啟

---

## Hamburger Menu 導航結構

```
≡  (NavigationView, PaneDisplayMode=LeftMinimal)
├── 🖩 計算機   → CalcPage (Visibility)
├── ⚙️  配置    → SettingsPage (Visibility)
└── ℹ️  關於    → AboutPage (Visibility)
```

- 頁面切換用 `Visibility`（不用 `Frame.Navigate`，避免重建 ViewModel）
- Header 區放 Toolbar：左側 App 標題，右側 `A | B` 雙欄開關

---

## 字串資源 Key 對照

| Key | 繁體 | 简体 |
|-----|------|------|
| `Nav_Calculator` | 計算機 | 计算器 |
| `Nav_Settings` | 配置 | 配置 |
| `Nav_About` | 關於 | 关于 |
| `Settings_Language` | 語言 | 语言 |
| `Settings_Theme` | 介面主題 | 界面主题 |
| `Settings_ThemeSystem` | 系統 | 系统 |
| `Settings_ThemeLight` | 明亮 | 明亮 |
| `Settings_ThemeDark` | 黑暗 | 深色 |
| `Error_DivZero` | 除數不可為零 | 除数不可为零 |
| `Error_Invalid` | 無效的輸入 | 无效的输入 |

新增字串時，**兩個 `.resw` 都要同步更新**，並在 `LocalizationService` 加對應 property。

---

## 開發規範

### 命名
- ViewModel property：PascalCase（`IsDualMode`、`Display`）
- Private field：`_camelCase`（`_isDualMode`、`_currentInput`）
- Service 靜態實例：`ServiceName.Instance`

### 按鈕事件（CalculatorView）
- Code-behind 只做一件事：`ViewModel.OnXxx()` 轉發
- 所有邏輯放在 `CalculatorViewModel` 的對應方法

### 新增按鈕流程
1. `CalculatorView.xaml` 加 `<Button>` 並綁 `Click`
2. `CalculatorView.xaml.cs` 加 handler，呼叫 ViewModel
3. `CalculatorViewModel.cs` 加對應的 `OnXxx()` 方法
4. 如需計算邏輯，加到 `CalculatorEngine.cs`

### 新增字串資源流程
1. `zh-Hant/Resources.resw` 新增 `<data name="Key">繁體</data>`
2. `zh-Hans/Resources.resw` 新增 `<data name="Key">简体</data>`
3. `LocalizationService.cs` 加 `public string Key => Get("Key");`
4. 在 `NotifyAllStrings()` 加 `OnPropertyChanged(nameof(Key));`

---

## 開發路線圖

| Phase | 內容 | 狀態 |
|-------|------|------|
| Phase 1 | 專案架構 + CalculatorEngine + 所有 ViewModel / Service / XAML 骨架 | ✅ 完成 |
| Phase 2 | UI 細節（動畫、按鈕 hover、雙欄展開動畫） | ⬜ 待開發 |
| Phase 3 | 鍵盤輸入支援 | ⬜ 待開發 |
| Phase 4 | 打包發布（MSIX / 免安裝版） | ⬜ 待開發 |

---

## 常見問題

**Q：為什麼運算子用 `×` `÷` 而不是 `*` `/`？**
A：XAML Button 顯示文字與 ViewModel Tag 保持一致，Shunting-yard 引擎也以這兩個符號為 key，避免轉換層。

**Q：為什麼頁面切換用 Visibility 而不是 Frame？**
A：Calculator A/B 的 ViewModel 狀態（算式、顯示值）需要在切換頁面後保留，`Frame.Navigate` 會重建 Page 導致狀態遺失。

**Q：LocalizationService 的字串是 runtime 讀 resw 嗎？**
A：是的，透過 `ResourceLoader` 在執行期讀取 `.resw`，切換語言時重新 `GetString(key)`，不需重啟 App。

---