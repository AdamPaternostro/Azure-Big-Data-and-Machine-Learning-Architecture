## This is for running a nested template locally
You need to sign in once and set your subscription once.

# Azure CLI (Linux)
## Login, set subscription and resource parameters (only need to do once per session)
```
az login
az account set -s 64a14e46-6c7d-4063-9665-c295287ab709
resourceGroup="Azure-Big-Data-Machine-Learning"
location="eastus"
az group create --name $resourceGroup --location $location
```

## Test each nested template seperately
You can run each of the below blocks from your local development machine
```
today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.databricks.json --parameters @local.parameters.databricks.json

today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.datafactory.json --parameters @local.parameters.datafactory.json

today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.datalake.json --parameters @local.parameters.datalake.json

today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.cosmosdb.json --parameters @local.parameters.cosmosdb.json

today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.storage.json --parameters @local.parameters.storage.json

today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.synapse-analytics.json --parameters @local.parameters.synapse-analytics.json

today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.analysisservices.json --parameters @local.parameters.analysisservices.json

today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"
az group deployment create --name $deploymentName --resource-group $resourceGroup --template-file azuredeploy.app-insights.json --parameters @local.parameters.app-insights.json
```

# Azure Powershell
## Login, set subscription and resource parameters (only need to do once per session)
```
Connect-AzAccount
$subscriptionId="64a14e46-6c7d-4063-9665-c295287ab709"
$context = Get-AzSubscription -SubscriptionId $subscriptionId
Set-AzContext $context
$resourceGroup="Azure-Big-Data-Machine-Learning"
$location="eastus"
New-AzResourceGroup -Name $resourceGroup -Location $location
```

## Test each nested template seperately
You can run each of the below blocks from your local development machine
```
$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.databricks.json -TemplateParameterFile local.parameters.databricks.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.datafactory.json -TemplateParameterFile local.parameters.datafactory.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.datalake.json -TemplateParameterFile local.parameters.datalake.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.cosmosdb.json -TemplateParameterFile local.parameters.cosmosdb.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.storage.json -TemplateParameterFile local.parameters.storage.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.synapse-analytics.json -TemplateParameterFile local.parameters.synapse-analytics.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.analysisservices.json -TemplateParameterFile local.parameters.analysisservices.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.app-insights.json -TemplateParameterFile local.parameters.app-insights.json
```