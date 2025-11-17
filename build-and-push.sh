#!/bin/bash

# Budowanie obrazów ARM64 dla Kubernetes na Raspberry Pi

export DOCKER_BUILDKIT=1

echo "🚀 Building images for Raspberry Pi Kubernetes (ARM64)"
echo "========================================================"

# Ustaw registry (zmień na swój)
REGISTRY="your-registry"  # np. "docker.io/username" lub "registry.local:5000"
VERSION="latest"

echo ""
echo "📦 Building API image..."
docker buildx build \
    --platform linux/arm64 \
    -t ${REGISTRY}/great-void-battle-api:${VERSION} \
    -f GreatVoidBattle.Api/Dockerfile \
    --push \
    .

if [ $? -ne 0 ]; then
    echo "❌ API build failed"
    exit 1
fi

echo ""
echo "📦 Building APP image..."
docker buildx build \
    --platform linux/arm64 \
    -t ${REGISTRY}/great-void-battle-app:${VERSION} \
    -f battle-app-admin/Dockerfile \
    --push \
    battle-app-admin

if [ $? -ne 0 ]; then
    echo "❌ APP build failed"
    exit 1
fi

echo ""
echo "✅ Images built and pushed successfully!"
echo ""
echo "📝 Update k8s manifests with registry:"
echo "   ${REGISTRY}/great-void-battle-api:${VERSION}"
echo "   ${REGISTRY}/great-void-battle-app:${VERSION}"
echo ""
echo "🚀 Deploy to Kubernetes:"
echo "   kubectl apply -f k8s/"
