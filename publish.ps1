# 設定發生錯誤時繼續執行，但顯示紅字
$ErrorActionPreference = "Continue"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "   開始清理專案目錄 (清理 bin 與 obj)    " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$binPath = "DualCalc\bin"
$objPath = "DualCalc\obj"

if (Test-Path $binPath) {
	Remove-Item -Recurse -Force $binPath -ErrorAction SilentlyContinue
	Write-Host "已刪除 $binPath"
}
if (Test-Path $objPath) {
	Remove-Item -Recurse -Force $objPath -ErrorAction SilentlyContinue
	Write-Host "已刪除 $objPath"
}

Write-Host "專案清理完成。" -ForegroundColor Green
Write-Host ""


Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "        開始打包 x64 單一執行檔          " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

dotnet publish DualCalc\DualCalc.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableMsixTooling=true -p:EnableDefaultPriItems=false -p:IncludeAllContentForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
	Write-Host "x64 打包過程發生錯誤，請檢查上方日誌。" -ForegroundColor Red
	exit $LASTEXITCODE
}
Write-Host "x64 打包成功！" -ForegroundColor Green
Write-Host ""


Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "       開始打包 arm64 單一執行檔         " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

dotnet publish DualCalc\DualCalc.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableMsixTooling=true -p:EnableDefaultPriItems=false -p:IncludeAllContentForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
	Write-Host "arm64 打包過程發生錯誤，請檢查上方日誌。" -ForegroundColor Red
	exit $LASTEXITCODE
}
Write-Host "arm64 打包成功！" -ForegroundColor Green
Write-Host ""


Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "          所有作業均已完成！             " -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "可於以下目錄找到發布的獨立檔案："
Write-Host "- x64   => DualCalc\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\"
Write-Host "- arm64 => DualCalc\bin\Release\net10.0-windows10.0.19041.0\win-arm64\publish\"
Write-Host "請直接複製上述 publish 資料夾進行發布部署。"