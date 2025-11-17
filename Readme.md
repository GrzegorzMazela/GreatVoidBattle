# Budowanie i publikowanie obrazów Docker

## Budowanie dla ARM64 (Raspberry Pi)

⚠️ **UWAGA**: Cross-kompilacja ARM64 na Windows może być niestabilna. Zalecane jest budowanie bezpośrednio na Raspberry Pi.

### Opcja 1: Budowanie na Raspberry Pi (ZALECANE)

Sklonuj repozytorium na Raspberry Pi i uruchom:

```bash
# API
docker build -t drazel/great-void-battle-api:0.1.1 -t drazel/great-void-battle-api:latest -f GreatVoidBattle.Api/Dockerfile .
docker push drazel/great-void-battle-api:0.1.1
docker push drazel/great-void-battle-api:latest

# App
docker build -t drazel/great-void-battle-app:0.1.2 -t drazel/great-void-battle-app:latest -f battle-app-admin/Dockerfile battle-app-admin
docker push drazel/great-void-battle-app:0.1.2
docker push drazel/great-void-battle-app:latest
```

### Opcja 2: Cross-kompilacja z Windows (eksperymentalne)

Jeśli musisz budować z Windows, spróbuj z większą pamięcią dla Docker Desktop:

```powershell
# Zwiększ pamięć w Docker Desktop Settings > Resources > Memory do min. 8GB

# API
docker buildx build --platform linux/arm64 -t drazel/great-void-battle-api:0.1.1 -t drazel/great-void-battle-api:latest -f GreatVoidBattle.Api/Dockerfile --push .

# App  
docker buildx build --platform linux/arm64 -t drazel/great-void-battle-app:0.1.2 -t drazel/great-void-battle-app:latest -f battle-app-admin/Dockerfile --push battle-app-admin
```

### Opcja 3: GitHub Actions CI/CD

Najlepsza opcja dla automatyzacji - GitHub Actions ma native ARM64 runners.

---

## Budowanie dla x86_64 (lokalny development)

### API
```
docker build -t drazel/great-void-battle-api:dev -f GreatVoidBattle.Api/Dockerfile .
```

### App
```
docker build -t drazel/great-void-battle-app:dev -f battle-app-admin/Dockerfile battle-app-admin
```

---
