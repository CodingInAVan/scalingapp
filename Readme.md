How to deploy in local.

```
docker build -t scalingapp:latest -f ScalingApi/Dockerfile .
docker build -t workernode:latest -f WorkerNode/Dockerfile .
minikube image load scalingapi:latest
minikube image load workernode:latest

cd deployment
helm install postgres-1 postgres-chart
helm install scalingapi scalingapi-chart
```