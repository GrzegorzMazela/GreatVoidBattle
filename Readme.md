# Budowanie i publikowanie obrazów Docker

## 1. GreatVoidBattle.Api

Zbuduj obraz z wersją (np. `1.0.0`) oraz jako `latest`:
```
docker build -t drazel/great-void-battle-api:0.1.0 -t drazel/great-void-battle-api:latest -f GreatVoidBattle.Api/Dockerfile .
```

Wystaw obraz do rejestru:
```
docker push drazel/great-void-battle-api:0.1.0
docker push drazel/great-void-battle-api:latest
```

---

## 2. GreatVoidBattle.App

Zbuduj obraz z wersją (np. `1.0.0`) oraz jako `latest`:
```
docker build -t drazel/great-void-battle-app:0.1.1 -t drazel/great-void-battle-app:latest -f battle-app-admin/Dockerfile battle-app-admin
```

Wystaw obraz do rejestru:
```
docker push drazel/great-void-battle-app:0.1.1
docker push drazel/great-void-battle-app:latest
```

---
