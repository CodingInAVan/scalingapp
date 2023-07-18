There are two components.

1. Scaling Api is to effectively manage active workers, which includes an endpoint for WorkerNodes to report their status to the server every minute.
2. WorkerNode
   The WorkerNode prints assigned job IDs and pings the API server to confirm its status. If it fails to ping for over two minutes, it is removed from the worker list in API. 

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