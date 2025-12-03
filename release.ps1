param([Parameter(Mandatory=$true)][string]$version)

Write-Host "🚀 Release v$version başlatılıyor..." -ForegroundColor Green

# 1. Temizlik
Write-Host "🗑️  Temizlik..." -ForegroundColor Yellow
Remove-Item -Path "publish" -Recurse -Force -ErrorAction SilentlyContinue

# 2. Build
Write-Host "📦 Build..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/

# 3. ZIP
Write-Host "📦 ZIP..." -ForegroundColor Yellow
cd publish
Compress-Archive -Path * -DestinationPath "../HisseAnalizUygulamasi-v$version.zip" -Force
cd ..

# 4. Git
Write-Host "📝 Git..." -ForegroundColor Yellow
git add .
git commit -m "v$version"
git tag "v$version"
git push origin main
git push origin "v$version"

Write-Host ""
Write-Host "✅ TAMAMLANDI!" -ForegroundColor Green
Write-Host ""
Write-Host "ŞİMDİ YAP:" -ForegroundColor Cyan
Write-Host "1. GitHub'a git ve Release oluştur" -ForegroundColor White
Write-Host "2. ZIP yükle: HisseAnalizUygulamasi-v$version.zip" -ForegroundColor White
Write-Host "3. update.xml'i güncelle (versiyon: $version.0)" -ForegroundColor Yellow
Write-Host ""