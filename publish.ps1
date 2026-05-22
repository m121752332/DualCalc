# 設定發生錯誤時繼續執行，但顯示紅字
$ErrorActionPreference = "Continue"

$publishOutputDir = "$PSScriptRoot\publish"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "   開始清理專案目錄與舊發佈檔案          " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$binPath = "$PSScriptRoot\DualCalc\bin"
$objPath = "$PSScriptRoot\DualCalc\obj"

if (Test-Path $binPath) {
	Remove-Item -Recurse -Force $binPath -ErrorAction SilentlyContinue
	Write-Host "已刪除 $binPath"
}
if (Test-Path $objPath) {
	Remove-Item -Recurse -Force $objPath -ErrorAction SilentlyContinue
	Write-Host "已刪除 $objPath"
}
if (Test-Path $publishOutputDir) {
	Remove-Item -Recurse -Force $publishOutputDir -ErrorAction SilentlyContinue
	Write-Host "已刪除 $publishOutputDir"
}

New-Item -ItemType Directory -Path $publishOutputDir -Force | Out-Null
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

$x64PublishDir = "$PSScriptRoot\DualCalc\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"
$x64ZipPath = "$publishOutputDir\DualCalc_win-x64.zip"
Write-Host "開始壓縮 x64 發佈檔案至: $x64ZipPath"
Compress-Archive -Path $x64PublishDir -DestinationPath $x64ZipPath -Force

Write-Host "x64 打包與壓縮成功！" -ForegroundColor Green
Write-Host ""


Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "       開始打包 arm64 單一執行檔         " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

dotnet publish DualCalc\DualCalc.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableMsixTooling=true -p:EnableDefaultPriItems=false -p:IncludeAllContentForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
	Write-Host "arm64 打包過程發生錯誤，請檢查上方日誌。" -ForegroundColor Red
	exit $LASTEXITCODE
}

$arm64PublishDir = "$PSScriptRoot\DualCalc\bin\Release\net10.0-windows10.0.19041.0\win-arm64\publish\*"
$arm64ZipPath = "$publishOutputDir\DualCalc_win-arm64.zip"
Write-Host "開始壓縮 arm64 發佈檔案至: $arm64ZipPath"
Compress-Archive -Path $arm64PublishDir -DestinationPath $arm64ZipPath -Force

Write-Host "arm64 打包與壓縮成功！" -ForegroundColor Green
Write-Host ""


Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "          所有作業均已完成！             " -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "可於以下目錄找到發布的壓縮檔案："
Write-Host "- $publishOutputDir"