# NOTE: You might need to wait 5 or 10 minutes after running this script to ensure permissions are proprogated

# SAS Token for ADF to copy data (public data)
# https://adampaternostropublic.blob.core.windows.net/public?se=2030-04-10T12%3A21%3A00Z&sp=rl&sv=2018-03-28&sr=c&sig=USm051L16GAz4bgQGhacK6t6BEFSexDxQ1DeeIAui5Q%3D

###########################################################
# Connect
###########################################################
# NOTE: You need to have these in environment variable
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


###########################################################
# Configure Managed Idenity for ADF to access the Data Lake
###########################################################
$dataFactoryName="datafactory00005"
$dataLakeName="datalake00005"

$Identity=(Get-AzDataFactoryV2 -ResourceGroupName $resourceGroup -Name $dataFactoryName).Identity
Write-Output $Identity.PrincipalId

$ServicePrincipal=(Get-AzADServicePrincipal -ObjectId $Identity.PrincipalId)
Write-Output $ServicePrincipal.Id

New-AzRoleAssignment -ObjectId $ServicePrincipal.Id `
    -RoleDefinitionName "Storage Blob Data Contributor" `
    -Scope  "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Storage/storageAccounts/$dataLakeName"


###########################################################
# Configure KeyVault for storage keys used by Function App
###########################################################
$keyVaultName="keyvault00005"
$functionAppName="functionapp00005"
$landingStorageAccountName="landingzonestorage00005"
$landingStorageAccountKey=""
$secretKeyLandingZoneStorageAccountName="LandingZoneStorageAccountName"
$secretKeyLandingZoneStorageAccountKey="LandingZoneStorageAccountKey"
$secretValueLandingZoneStorageAccountName=""
$secretValueLandingZoneStorageAccountKey=""

# Set our deployment service principle such it has access to set the secret
Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ServicePrincipalName $spApplicationId -PermissionsToKeys create,import,delete,list -PermissionsToSecrets set,delete -PassThru

# Push the secrets
$secretValueLandingZoneStorageAccountName = ConvertTo-SecureString $landingStorageAccountName -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $secretKeyLandingZoneStorageAccountName -SecretValue $secretValueLandingZoneStorageAccountName

$landingStorageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroup -AccountName $landingStorageAccountName).Value[0]
$secretValueLandingZoneStorageAccountKey = ConvertTo-SecureString $landingStorageAccountKey -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $secretKeyLandingZoneStorageAccountKey -SecretValue $secretValueLandingZoneStorageAccountKey

# Grant the Function App access to read the secret
# Get the MSI id from the Function App
# Get the Function App Name
$functionAppSPAppId=$(Get-AzADServicePrincipal -DisplayName $functionAppName).ApplicationId
Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ServicePrincipalName $functionAppSPAppId -PermissionsToSecrets get  -PassThru
