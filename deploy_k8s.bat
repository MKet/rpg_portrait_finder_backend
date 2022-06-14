SETLOCAL
set sa_password="MyC0m9l&xP@ssw0rd"
kubectl create secret generic mssql --from-literal=SA_PASSWORD=%sa_password%

kubectl apply -f 3rd-party-k8s/pvc.yaml
kubectl apply -f 3rd-party-k8s/mssql-deployment.yaml

kubectl apply -f 3rd-party-k8s/zookeeper-deployment.yaml
kubectl apply -f 3rd-party-k8s/zookeeper-service.yaml
kubectl apply -f 3rd-party-k8s/kafka-deployment.yaml
kubectl apply -f 3rd-party-k8s/kafka-service.yaml

:: setup ApiGateway
docker build -t localhost:5001/apigateway:latest -f ./ApiGateway/Dockerfile .
docker push localhost:5001/apigateway:latest

kubectl apply -f ApiGateway/apigateway-deployment.yaml
kubectl apply -f ApiGateway/apigateway-service.yaml
kubectl apply -f ApiGateway/apigateway-ingress.yaml

:: setup Authentication Service
docker build -t localhost:5001/authenticationservice:latest -f ./AuthenticationService/Dockerfile .
docker push localhost:5001/authenticationservice:latest

kubectl create secret generic authenticationservice --from-literal=ConnectionString="mssql-deployment:1433;Initial Catalog=AuthenticationService;Username=SA;Password="+%sa_password%
kubectl apply -f AuthenticationService/authenticationservice-deployment.yaml
kubectl apply -f AuthenticationService/authenticationservice-service.yaml
kubectl apply -f AuthenticationService/authenticationservice-autoscaler.yaml
ENDLOCAL
pause