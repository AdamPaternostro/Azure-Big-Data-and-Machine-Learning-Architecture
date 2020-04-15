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
# Currently this needs to be an SP for
# Owner of CosmosDB to get the connection keys
# Be able query Azure AD to get the system MSI object ids for setting permissions
$servicePrincipal = New-AzADServicePrincipal -DisplayName "BigDataMachineLearningSP" `
                    -Role Owner -Scope /subscriptions/$subscriptionId/resourceGroups/$resourceGroup

# Decrypt the password
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($servicePrincipal.Secret)
$password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)


####################################################
# Write the variables to a file so you can run in other scripts
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
$env:resourceGroup = $resourceGroup


Get-ChildItem Env:spPassword
Get-ChildItem Env:spApplicationId
Get-ChildItem Env:spId
Get-ChildItem Env:subscriptionId
Get-ChildItem Env:tenantId
Get-ChildItem Env:resourceGroup


# Create the file to be called as the first step of the other scripts
$output='$env:spPassword = "' + $password  + '"'
Write-Output  $output > STEP-00-SetEnvironmentVariables.ps1

$output='$env:spApplicationId = "' + $servicePrincipal.ApplicationId  + '"'
Write-Output  $output >> STEP-00-SetEnvironmentVariables.ps1

$output='$env:spId = "' + $servicePrincipal.Id  + '"'
Write-Output  $output >> STEP-00-SetEnvironmentVariables.ps1

$output='$env:subscriptionId = "' + $subscriptionId  + '"'
Write-Output  $output >> STEP-00-SetEnvironmentVariables.ps1

$output='$env:tenantId = "' + $context.TenantId  + '"'
Write-Output  $output >> STEP-00-SetEnvironmentVariables.ps1

$output='$env:resourceGroup = "' + $resourceGroup  + '"'
Write-Output  $output >> STEP-00-SetEnvironmentVariables.ps1
