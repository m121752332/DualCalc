# 更新日誌 (Changelog)

此文件記錄 DualCalc 雙欄計算機的異動歷程，包含新功能 (New Features) 與優化 (Optimizations)。

## [Unreleased / Recent Updates]

### 🚀 新功能 (New Features)

* **多語系架構升級與擴展**：
  - 徹底重構 `LocalizationService.cs`，現在支援直接自獨立的 JSON 檔案讀取多國語系設定。
  - 新增擴展對「英文 (`en-US`)」與「日文 (`ja-JP`)」的語系支援，並於 `SettingsView` 增加相對應的切換選項。
  - 語系服務會嘗試讀取外部放置的 JSON 語系字典 (`i18n/{lang}.json`)，提供更大彈性且無須依賴系統內建 ResW API。

### 🔧 優化 (Optimizations)

* **設定頁面 (Settings) 體驗優化**：
  - 新增載入錯誤提示（Error InfoBar），當嘗試載入或解析 JSON 語系檔失敗時，會在設定頁面跳出警告與清楚的失敗原因，提供更好的除錯體驗。
  - 同步重構 `SettingsViewModel` 支援擴展的語系綁定。
* **雙欄切換動態顯示邏輯優化**：
  - 更新 `MainViewModel` 中的多語系字串讀取，修正開啟/關閉第二台計算機的懸浮提示 (ToolTip) 與內容文案，使其能支援新版動態語系架構讀取到的內容。
- 移除不再需要的 YamlDotNet 相關語系邏輯，全面專注於 JSON 與 MVVM 多語系綁定。  - 在 `LocalizationService` 統一將 YamlDotNet 的 `Deserializer` 初始邏輯封裝與重用，提升了運作效能。