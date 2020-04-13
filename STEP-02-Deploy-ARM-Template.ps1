# Change to your Nested directory
Set-Location C:\Azure-Big-Data-and-Machine-Learning-Architecture\ARM-Templates\nested

###########################################################
# Connect
###########################################################
# NOTE: You need to have these in environment variables
# $env:spPassword = "REPLACE_ME"
# $env:spApplicationId = "REPLACE_ME"
# $env:spId = "REPLACE_ME"
# $env.subscriptionId = "REPLACE_ME"

$tenantId=$(Get-ChildItem Env:tenantId).Value
$subscriptionId=$(Get-ChildItem Env:subscriptionId).Value
$spPassword=$(Get-ChildItem Env:spPassword).Value
$spApplicationId=$(Get-ChildItem Env:spApplicationId).Value
$spId=$(Get-ChildItem Env:spId).Value

Write-Output $tenantId
Write-Output $subscriptionId
Write-Output $spPassword
Write-Output $spApplicationId
Write-Output $spId

# Get the subscription and then login using the service principal
$securePassword = ConvertTo-SecureString $spPassword -AsPlainText -Force
$psCredential = New-Object System.Management.Automation.PSCredential($spApplicationId , $securePassword)
Connect-AzAccount -ServicePrincipal -SubscriptionId $subscriptionId -Tenant $tenantId -Credential $psCredential

$context = Get-AzSubscription -SubscriptionId $subscriptionId
Set-AzContext $context


###########################################################
# Script parameters
###########################################################
$resourceGroup="Azure-Big-Data-Machine-Learning"


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

# Depends on Storage, App Insights, Container Registry and KeyVault
$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.machinelearning.json -TemplateParameterFile local.parameters.machinelearning.json

# Depends on Storage, App Insights
$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.app-service.json -TemplateParameterFile local.parameters.app-service.json