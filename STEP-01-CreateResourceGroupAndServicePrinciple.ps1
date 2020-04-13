# Description: This will create a service principal that will be used for deploying and managing all resources

####################################################
# Manually connect (you need to be a admin of the subscription)
####################################################
Connect-AzAccount


####################################################
# Select Subscription
####################################################
$subscriptionId="REPLACE-ME"
$context = Get-AzSubscription -SubscriptionId $subscriptionId
Set-AzContext $context


####################################################
# Create resource group
####################################################
$resourceGroup="Azure-Big-Data-Machine-Learning"
$location="eastus"
New-AzResourceGroup -Name $resourceGroup -Location $location


####################################################
# Create service pricipal as owner of our resource group
####################################################
$servicePrincipal = New-AzADServicePrincipal -DisplayName "BigDataMachineLearningSP" -Role Owner -Scope /subscriptions/$subscriptionId/resourceGroups/$resourceGroup

# Decrypt the password
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($servicePrincipal.Secret)
$password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# NOTE: We need to set the service principle to query Azure AD (e.g. Get-AzADServicePrincipal)
#New-AzRoleAssignment -ObjectId $servicePrincipal.ApplicationId `
#-RoleDefinitionName "Virtual Machine Contributor" `
#-ResourceName Devices-Engineering-ProjectRND `
#-ResourceType Microsoft.Network/virtualNetworks/subnets `
#-ParentResource virtualNetworks/VNET-EASTUS-01 `
#-ResourceGroupName Network


####################################################
# NOTE
# YOU NEED TO SAVE THESE!!!!!
####################################################
Write-Output $password
Write-Output $servicePrincipal.ApplicationId
Write-Output $servicePrincipal.Id

# Place in Environment variables
$env:spPassword = $password
$env:spApplicationId = $servicePrincipal.ApplicationId
$env:spId = $servicePrincipal.Id
$env:subscriptionId = $subscriptionId
$env:tenantId = $context.TenantId

Get-ChildItem Env:spPassword
Get-ChildItem Env:spApplicationId
Get-ChildItem Env:spId
Get-ChildItem Env:subscriptionId
Get-ChildItem Env:tenantId



