#!/bin/bash
# Skrypt do budowania obrazów bezpośrednio na Raspberry Pi

set -e  # Exit on error

REGISTRY="drazel"
API_VERSION="0.1.1"
APP_VERSION="0.1.2"

echo "🚀 Building images natively on Raspberry Pi (ARM64)"
echo "====================================================="

# Login do Docker Hub (jeśli potrzebne)
echo ""
read -p "Do you want to login to Docker Hub? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker login
fi

# Build API
echo ""
echo "📦 Building API image..."
docker build \
    -t ${REGISTRY}/great-void-battle-api:${API_VERSION} \
    -t ${REGISTRY}/great-void-battle-api:latest \
    -f GreatVoidBattle.Api/Dockerfile \
    .

if [ $? -ne 0 ]; then
    echo "❌ API build failed"
    exit 1
fi

# Build App
echo ""
echo "📦 Building App image..."
docker build \
    -t ${REGISTRY}/great-void-battle-app:${APP_VERSION} \
    -t ${REGISTRY}/great-void-battle-app:latest \
    -f battle-app-admin/Dockerfile \
    battle-app-admin

if [ $? -ne 0 ]; then
    echo "❌ App build failed"
    exit 1
fi

# Push images
echo ""
echo "📤 Pushing images to registry..."

docker push ${REGISTRY}/great-void-battle-api:${API_VERSION}
docker push ${REGISTRY}/great-void-battle-api:latest
docker push ${REGISTRY}/great-void-battle-app:${APP_VERSION}
docker push ${REGISTRY}/great-void-battle-app:latest

echo ""
echo "✅ Build and push completed successfully!"
echo ""
echo "📝 Images available:"
echo "   ${REGISTRY}/great-void-battle-api:${API_VERSION}"
echo "   ${REGISTRY}/great-void-battle-api:latest"
echo "   ${REGISTRY}/great-void-battle-app:${APP_VERSION}"
echo "   ${REGISTRY}/great-void-battle-app:latest"
echo ""
echo "🚀 Deploy to Kubernetes:"
echo "   kubectl apply -f k8s/"
