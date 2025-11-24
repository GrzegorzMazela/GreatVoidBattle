# Great Void Battle - Kubernetes Deployment na Raspberry Pi

## Wymagania

- Klaster Kubernetes na Raspberry Pi (K3s, MicroK8s lub kubeadm)
- kubectl skonfigurowany do komunikacji z klastrem
- Docker registry dostępny dla klastra (Docker Hub, Harbor, lub lokalny registry)
- Ingress controller (np. nginx-ingress)

## Quick Start

### 1. Przygotowanie obrazów

**ZALECANE: Budowanie na Raspberry Pi**

Sklonuj repozytorium na Raspberry Pi i zbuduj obrazy natywnie:

```bash
# Sklonuj repo
git clone <your-repo-url>
cd GreatVoidBattle

# Zaloguj się do Docker Hub
docker login

# Zbuduj i wypchnij obrazy
chmod +x build-on-rpi.sh
./build-on-rpi.sh
```

Obrazy będą automatycznie zbudowane dla ARM64 (natywnie) i wypchnięte do registry.

**ALTERNATYWA: Cross-kompilacja z Windows (niestabilne)**

Jeśli musisz budować z Windows:
1. Zwiększ pamięć Docker Desktop do min. 8GB (Settings > Resources)
2. Uruchom `.\build-and-push.ps1`

⚠️ Cross-kompilacja .NET na ARM64 może być niestabilna i kończyć się błędami emulacji.

### 2. Zaktualizuj manifesty Kubernetes (jeśli potrzebne)

Manifesty są już skonfigurowane dla `drazel` registry. Jeśli używasz innego registry, edytuj:
- `k8s/api-deployment.yaml` - zmień `image: drazel/great-void-battle-api:latest`
- `k8s/app-deployment.yaml` - zmień `image: drazel/great-void-battle-app:latest`

### 3. Deploy na klaster

```bash
# Zastosuj wszystkie manifesty
kubectl apply -f k8s/

# Lub krok po kroku:
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/mongodb-pvc.yaml
kubectl apply -f k8s/mongodb.yaml
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/app-deployment.yaml
kubectl apply -f k8s/ingress.yaml
```

## Struktura manifestów

```
k8s/
├── namespace.yaml          # Namespace: great-void-battle
├── mongodb-pvc.yaml        # PersistentVolumeClaim dla MongoDB
├── mongodb.yaml            # MongoDB Deployment + Service
├── api-deployment.yaml     # API Deployment + Service
├── app-deployment.yaml     # UI Deployment + Service
└── ingress.yaml           # Ingress routing
```

## Zarządzanie aplikacją

### Sprawdzenie statusu
```bash
kubectl get all -n great-void-battle
```

### Logi
```bash
# API
kubectl logs -n great-void-battle -l app=great-void-battle-api -f

# UI
kubectl logs -n great-void-battle -l app=great-void-battle-app -f

# MongoDB
kubectl logs -n great-void-battle -l app=mongodb -f
```

### Skalowanie
```bash
# Skaluj API
kubectl scale deployment great-void-battle-api -n great-void-battle --replicas=3

# Skaluj UI
kubectl scale deployment great-void-battle-app -n great-void-battle --replicas=3
```

### Restart deploymentu
```bash
kubectl rollout restart deployment/great-void-battle-api -n great-void-battle
kubectl rollout restart deployment/great-void-battle-app -n great-void-battle
```

### Aktualizacja aplikacji
```bash
# 1. Zbuduj nowe obrazy z nową wersją
REGISTRY="your-registry"
VERSION="v1.0.1"

docker buildx build --platform linux/arm64 \
    -t ${REGISTRY}/great-void-battle-api:${VERSION} \
    -f GreatVoidBattle.Api/Dockerfile --push .

docker buildx build --platform linux/arm64 \
    -t ${REGISTRY}/great-void-battle-app:${VERSION} \
    -f battle-app-admin/Dockerfile --push battle-app-admin

# 2. Zaktualizuj deployment
kubectl set image deployment/great-void-battle-api \
    api=${REGISTRY}/great-void-battle-api:${VERSION} \
    -n great-void-battle

kubectl set image deployment/great-void-battle-app \
    app=${REGISTRY}/great-void-battle-app:${VERSION} \
    -n great-void-battle
```

## Dostęp do aplikacji

### Przez Ingress (domyślnie)
```bash
# Dodaj do /etc/hosts (na komputerze, z którego łączysz się)
echo "<RASPBERRY_PI_IP> gvb.local" | sudo tee -a /etc/hosts

# Otwórz w przeglądarce
http://gvb.local
```

### Przez NodePort (alternatywa)

Jeśli nie masz Ingress, zmień Service type na NodePort:

```yaml
# api-deployment.yaml i app-deployment.yaml
spec:
  type: NodePort
  ports:
  - port: 8080
    nodePort: 30080  # dla API
```

Dostęp: `http://<RASPBERRY_PI_IP>:30080`

## Lokalny Docker Registry (opcjonalnie)

Jeśli chcesz używać lokalnego registry w klastrze:

### 1. Uruchom registry
```bash
kubectl apply -f - <<EOF
apiVersion: apps/v1
kind: Deployment
metadata:
  name: docker-registry
  namespace: kube-system
spec:
  replicas: 1
  selector:
    matchLabels:
      app: docker-registry
  template:
    metadata:
      labels:
        app: docker-registry
    spec:
      containers:
      - name: registry
        image: registry:2
        ports:
        - containerPort: 5000
---
apiVersion: v1
kind: Service
metadata:
  name: docker-registry
  namespace: kube-system
spec:
  selector:
    app: docker-registry
  ports:
  - port: 5000
  type: NodePort
EOF
```

### 2. Skonfiguruj Docker do pracy z insecure registry
```bash
# Na nodach klastra i maszynie budującej
sudo nano /etc/docker/daemon.json
```

Dodaj:
```json
{
  "insecure-registries": ["<RASPBERRY_PI_IP>:5000"]
}
```

```bash
sudo systemctl restart docker
```

### 3. Użyj lokalnego registry
```bash
REGISTRY="<RASPBERRY_PI_IP>:5000"
```

## Monitoring

### Zasoby
```bash
# CPU i pamięć podów
kubectl top pods -n great-void-battle

# CPU i pamięć nodów
kubectl top nodes
```

### Events
```bash
kubectl get events -n great-void-battle --sort-by='.lastTimestamp'
```

## Backup MongoDB

### Ręczny backup
```bash
# Eksport
kubectl exec -n great-void-battle deployment/mongodb -- \
    mongodump --out /tmp/backup

# Kopiowanie z poda
kubectl cp great-void-battle/mongodb-pod:/tmp/backup ./mongodb-backup
```

### CronJob do automatycznego backupu
```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: mongodb-backup
  namespace: great-void-battle
spec:
  schedule: "0 2 * * *"  # Codziennie o 2:00
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: backup
            image: mongo:7.0
            command:
            - /bin/sh
            - -c
            - mongodump --host=mongodb --out=/backup/$(date +%Y%m%d)
            volumeMounts:
            - name: backup-storage
              mountPath: /backup
          restartPolicy: OnFailure
          volumes:
          - name: backup-storage
            persistentVolumeClaim:
              claimName: mongodb-backup-pvc
```

## Troubleshooting

### Pod nie startuje
```bash
kubectl describe pod <pod-name> -n great-void-battle
kubectl logs <pod-name> -n great-void-battle
```

### ImagePullBackOff
```bash
# Sprawdź czy registry jest dostępny
# Sprawdź czy imagePullSecrets są skonfigurowane (jeśli registry prywatne)
```

### MongoDB brak miejsca
```bash
# Zwiększ PVC
kubectl edit pvc mongodb-pvc -n great-void-battle
# Zmień storage: 10Gi na większy rozmiar
```

## Usuwanie aplikacji

```bash
# Usuń wszystko oprócz PVC (zachowa dane MongoDB)
kubectl delete -f k8s/ingress.yaml
kubectl delete -f k8s/app-deployment.yaml
kubectl delete -f k8s/api-deployment.yaml
kubectl delete -f k8s/mongodb.yaml

# Usuń wszystko łącznie z danymi
kubectl delete namespace great-void-battle
```
