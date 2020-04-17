# This file should have been created for you by the STEP-01-CreateResourceGroupAndServicePrinciple.ps1 script
Set-Location C:\Azure-Big-Data-and-Machine-Learning-Architecture\
.\STEP-00-SetEnvironmentVariables.ps1

Set-Location C:\Azure-Big-Data-and-Machine-Learning-Architecture\ARM-Templates\nested

###########################################################
# Connect
###########################################################
$tenantId=$(Get-ChildItem Env:tenantId).Value
$subscriptionId=$(Get-ChildItem Env:subscriptionId).Value
$spPassword=$(Get-ChildItem Env:spPassword).Value
$spApplicationId=$(Get-ChildItem Env:spApplicationId).Value
$spId=$(Get-ChildItem Env:spId).Value
$resourceGroup=$(Get-ChildItem Env:resourceGroup).Value

Write-Output $tenantId
Write-Output $subscriptionId
Write-Output $spPassword
Write-Output $spApplicationId
Write-Output $spId
Write-Output $resourceGroup

# Get the subscription and then login using the service principal
$securePassword = ConvertTo-SecureString $spPassword -AsPlainText -Force
$psCredential = New-Object System.Management.Automation.PSCredential($spApplicationId , $securePassword)

Connect-AzAccount -ServicePrincipal -SubscriptionId $subscriptionId -Tenant $tenantId -Credential $psCredential
#Connect-AzAccount

$context = Get-AzSubscription -SubscriptionId $subscriptionId
Set-AzContext $context


###########################################################
# Script parameters
###########################################################



####################################################
# Test each nested template seperately (you can run one by one)
####################################################

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
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.storage-containers.json -TemplateParameterFile local.parameters.storage-containers.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.synapse-analytics.json -TemplateParameterFile local.parameters.synapse-analytics.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.analysisservices.json -TemplateParameterFile local.parameters.analysisservices.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.app-insights.json -TemplateParameterFile local.parameters.app-insights.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.keyvault.json -TemplateParameterFile local.parameters.keyvault.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.container-registry.json -TemplateParameterFile local.parameters.container-registry.json

$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.datashare.json -TemplateParameterFile local.parameters.datashare.json

# Depends on Storage, App Insights, Container Registry and KeyVault
$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.machinelearning.json -TemplateParameterFile local.parameters.machinelearning.json

# Depends on Storage, App Insights
$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.app-service.json -TemplateParameterFile local.parameters.app-service.json

# Depends on Storage and Storage Containers
$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.event-grid.json -TemplateParameterFile local.parameters.event-grid.json
