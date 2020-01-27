docker build -t AA -f Dockerfile .
call az acr login --name AA
docker tag AA AA.azurecr.io/serverlessorleansproxy
docker push AA.azurecr.io/serverlessorleansproxy

set ACR_NAME=serverlessorleansacr
set sp_Name=orleanswcesp

rem az ad sp create-for-rbac --name http://%sp_Name% --scopes %ACR_registry_id% --role acrpull --query password --output tsv
rem az ad sp show --id http://%sp_Name% --query appId --output tsv

set acrpswd=AA
set acrappid=AA

call az container create --resource-group serverlessorleans --name serverlessorleansproxy --image AA.azurecr.io/serverlessorleansproxy --cpu 1 --memory 1 --registry-login-server serverlessorleansacr.azurecr.io --registry-username %acrappid% --registry-password %acrpswd% --dns-name-label serverlessorleansproxy --ports 80 443 --assign-identity --restart-policy OnFailure
call az container show --resource-group serverlessorleans --name serverlessorleansproxy --query instanceView.state

call az container restart --resource-group serverlessorleans --name serverlessorleansproxy
call az container logs --resource-group serverlessorleans --name serverlessorleansproxy

