# Azure-Big-Data-and-Machine-Learning-Architecture
This is the set of ARM templates that will deploy this architecture

## How to run
* Download azuredeploy.parameters.json
* Make your edits
* Click the Deploy to Azure button
* Click "Edit Parameters" and upload your azuredeploy.parameters.json


## Overview of Templates
The below diagram shows the layout of the templates.  The templates have been nested such that each nested template can be tested independently and then linked in the master template.  The master template will pass the parameters to each linked template and reference an dependencies between the linked templates.

![alt tag](https://raw.githubusercontent.com/adampaternostro/Azure-Sample-ARM-Template-Architecture/master/ARM-Architecture.png)

### Specific template notes
*  None


## To Run via command line (Linux)
```
# Login
az login

# Select Subscription
az account set -s REPLACE_ME

# Script parameters
resourceGroup="Azure-Big-Data-Machine-Learning"
location="eastus"
today=`date +%Y-%m-%d-%H-%M-%S`
deploymentName="MyDeployment-$today"

# Create resource group
az group create \
  --name        $resourceGroup \
  --location    $location

# Deploy the ARM template
az group deployment create \
  --name                 $deploymentName \
  --resource-group       $resourceGroup \
  --template-file        azuredeploy.json \
  --parameters           @azuredeploy.parameters.json \
  --mode                 Incremental

# Clean up resource group
az group delete --name $resourceGroup
```


## To Run via command line (Powersehll)
```
# Login
Connect-AzAccount

# Select Subscription
$subscriptionId="REPLACE_ME"
$context = Get-AzSubscription -SubscriptionId $subscriptionId
Set-AzContext $context

# Script parameters
$resourceGroup="Azure-Big-Data-Machine-Learning"
$location="eastus"
$today=(Get-Date).ToString('yyyy-MM-dd-HH-mm-ss')
$deploymentName="MyDeployment-$today"

# Create resource group
New-AzResourceGroup -Name $resourceGroup -Location $location

# Deploy the ARM template
New-AzResourceGroupDeployment -Name $deploymentName -ResourceGroupName $resourceGroup -TemplateFile azuredeploy.json -TemplateParameterFile parameters.parameters.json -Mode Incremental

# Clean up resource group
Remove-AzResourceGroup -Name $resourceGroup 
```



## References
* Each Azure Resource Reference: https://docs.microsoft.com/en-us/azure/templates/
* Quickstart templates: https://github.com/Azure/azure-quickstart-templates