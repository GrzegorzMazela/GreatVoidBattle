# Budowanie i publikowanie obrazów Docker

## Budowanie dla ARM64 (Raspberry Pi)

⚠️ **UWAGA**: Cross-kompilacja ARM64 na Windows może być niestabilna. Zalecane jest budowanie bezpośrednio na Raspberry Pi.

### Opcja 1: Budowanie na Raspberry Pi (ZALECANE)

Sklonuj repozytorium na Raspberry Pi i uruchom:

```bash
# API
docker build -t drazel/great-void-battle-api:0.1.4 -t drazel/great-void-battle-api:latest -f GreatVoidBattle.Api/Dockerfile .
docker push drazel/great-void-battle-api:0.1.4
docker push drazel/great-void-battle-api:latest

# App
docker build -t drazel/great-void-battle-app:0.1.8 -t drazel/great-void-battle-app:latest -f battle-app-admin/Dockerfile battle-app-admin
docker push drazel/great-void-battle-app:0.1.8
docker push drazel/great-void-battle-app:latest
```


## Restart Kubernetes

### APP
kubectl rollout restart deployment great-void-battle-app

### API
kubectl rollout restart deployment great-void-battle-api