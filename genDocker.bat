az account set --subscription xxd33833-2ab5-4460-ba7d-xx4b5135c24a

dotnet build OrleansSharedInterface -c ReleaseMe1 --no-incremental
dotnet build OrleansSharedGrains -c ReleaseMe1 --no-incremental
dotnet build OrleansSiloHost -c ReleaseMe1 --no-incremental

cd "C:\DataStore\Source Code\Orleans\OrleansSiloHost" 

dotnet build -c ReleaseMe1 --no-incremental
dotnet publish -c ReleaseMe1
docker build -t serverlessorleanssilogeneric -f Dockerfile .

rem az group create --name ServerlessOrleans --location UKSouth
rem az acr create --resource-group ServerlessOrleans --name serverlessorleansacr --sku Basic --admin-enabled true
call az acr login --name serverlessorleansacr
REM az acr show --name serverlessorleansacr --query loginServer --output table

docker tag serverlessorleanssilo serverlessorleansacr.azurecr.io/serverlessorleanssilogeneric
docker push serverlessorleansacr.azurecr.io/serverlessorleanssilogeneric

set ACR_NAME=serverlessorleansacr
set sp_Name=orleanswcesp

rem az ad sp create-for-rbac --name http://%sp_Name% --scopes %ACR_registry_id% --role acrpull --query password --output tsv
rem  az ad sp show --id http://%sp_Name% --query appId --output tsv

set acrpswd=7c89122c-863d-462e-8db0-3da6dcb08435
set acrappid=e0d05cf3-a0a8-4b65-b385-9e5d5f91ef45

call az container create --resource-group serverlessorleansgeneric --name serverlessorleanssilo1generic --image serverlessorleansacr.azurecr.io/serverlessorleanssilogeneric --restart-policy OnFailure --cpu 2 --memory 1 --registry-login-server serverlessorleansacr.azurecr.io --registry-username %acrappid% --registry-password %acrpswd% --dns-name-label serverlessorleanssilo1generic --ports 20010 33350 --assign-identity
rem call az container show --resource-group serverlessorleans --name serverlessorleanssilo1 --query instanceView.state
call az container stop --resource-group serverlessorleans --name serverlessorleanssilo1
call az container start --resource-group serverlessorleans --name serverlessorleanssilo1 --no-wait
call az container logs --resource-group serverlessorleans --name serverlessorleanssilo1 --follow

REM Now publish the Server Gateway...
cd "C:\DataStore\Source Code\Orleans\OrleansClientFunctions"

az storage account create --resource-group ServerlessOrleansGeneric --name sorlgen
az functionapp create --resource-group ServerlessOrleansGeneric --os-type Windows --runtime dotnet --name 
ServerlessGWGeneric --storage-account sorlgen --consumption-plan-location UKSouth
func azure functionapp publish ServerlessGWgeneric  

cd "C:\DataStore\Source Code\Orleans\" 
call az container logs --resource-group serverlessorleansc --name serverlessorleanssilo1generic --follow


