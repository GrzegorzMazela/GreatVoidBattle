# Budowanie obrazów ARM64 dla Kubernetes na Raspberry Pi

# Włącz BuildKit
$env:DOCKER_BUILDKIT = 1

Write-Host "🚀 Building images for Raspberry Pi Kubernetes (ARM64)" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green

# Ustaw registry (zmień na swój)
$REGISTRY = "drazel"  # np. "docker.io/username" lub "registry.local:5000"
$VERSION = "0.1.2"

Write-Host ""
Write-Host "📦 Building API image..." -ForegroundColor Cyan
docker buildx build `
    --platform linux/arm64 `
    -t ${REGISTRY}/great-void-battle-api:${VERSION} `
    -f GreatVoidBattle.Api/Dockerfile `
    --push `
    .

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ API build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "📦 Building APP image..." -ForegroundColor Cyan
docker buildx build `
    --platform linux/arm64 `
    -t ${REGISTRY}/great-void-battle-app:${VERSION} `
    -f battle-app-admin/Dockerfile `
    --push `
    battle-app-admin

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ APP build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Images built and pushed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Update k8s manifests with registry:" -ForegroundColor Yellow
Write-Host "   ${REGISTRY}/great-void-battle-api:${VERSION}" -ForegroundColor White
Write-Host "   ${REGISTRY}/great-void-battle-app:${VERSION}" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Deploy to Kubernetes:" -ForegroundColor Yellow
Write-Host "   kubectl apply -f k8s/" -ForegroundColor White
