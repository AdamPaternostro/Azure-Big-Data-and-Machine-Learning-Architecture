# NOTE: You might need to wait 5 or 10 minutes after running this script to ensure permissions are proprogated

# This file should have been created for you by the STEP-01-CreateResourceGroupAndServicePrinciple.ps1 script
Set-Location C:\Azure-Big-Data-and-Machine-Learning-Architecture\
.\STEP-00-SetEnvironmentVariables.ps1

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

#Connect-AzAccount -ServicePrincipal -SubscriptionId $subscriptionId -Tenant $tenantId -Credential $psCredential
Connect-AzAccount

$context = Get-AzSubscription -SubscriptionId $subscriptionId
Set-AzContext $context


###########################################################
# Script parameters
###########################################################
$dataFactoryName="datafactory00005"
$dataLakeName="datalake00005"
$keyVaultName="keyvault00005"
$functionAppName="functionapp00005"
$landingStorageAccountName="landingzonestorage00005"
$cosmosDbName="processingestion00005"


###########################################################
# Configure Managed Idenity for ADF to access the Data Lake adn Landing Zone storage
###########################################################
$Identity=(Get-AzDataFactoryV2 -ResourceGroupName $resourceGroup -Name $dataFactoryName).Identity
Write-Output $Identity.PrincipalId

$ServicePrincipal=(Get-AzADServicePrincipal -ObjectId $Identity.PrincipalId)
Write-Output $ServicePrincipal.Id

# ERROR: This does not work when using a service principal to connect to Azure.  It says object id is ''
# Cannot validate argument on parameter 'ObjectId'. The argument is null or empty. Provide an argument that is not null or empty, and then try the command again.
New-AzRoleAssignment -ObjectId $ServicePrincipal.Id `
    -RoleDefinitionName "Storage Blob Data Contributor" `
    -Scope  "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Storage/storageAccounts/$dataLakeName"

# ERROR: This does not work when using a service principal to connect to Azure.  It says object id is ''
# Cannot validate argument on parameter 'ObjectId'. The argument is null or empty. Provide an argument that is not null or empty, and then try the command again.
New-AzRoleAssignment -ObjectId $ServicePrincipal.Id `
    -RoleDefinitionName "Storage Blob Data Contributor" `
    -Scope  "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Storage/storageAccounts/$landingStorageAccountName"


###########################################################
# Configure KeyVault for storage keys used by Function App
###########################################################
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
# ERROR: This does not work when using a service principal to connect to Azure.  It says not authorized
# Get-AzADServicePrincipal : Insufficient privileges to complete the operation.
$functionAppSPAppId=$(Get-AzADServicePrincipal -DisplayName $functionAppName).ApplicationId
Write-Output functionAppSPAppId
Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ServicePrincipalName $functionAppSPAppId -PermissionsToSecrets get  -PassThru


###########################################################
# Configure Blob Storage (landing zone) to push events to an AZure Function
###########################################################
$storageContext = $(New-AzStorageContext -StorageAccountName $landingStorageAccountName -UseConnectedAccount)

# Create queue "fileevent"
New-AzStorageQueue -Name "fileevent" -Context $storageContext


###########################################################
# Assign Azure Function MSI to access CosmosDB
###########################################################
$func=(Get-AzWebApp -ResourceGroupName $resourceGroup -Name $functionAppName)
$functionAppSPAppId=$func.Identity.PrincipalId

# Must be a CosmosDB Owner to use MSI to get the keys
# ERROR: This does not work when using a service principal to connect to Azure.  It says object id is ''
# Cannot validate argument on parameter 'ObjectId'. The argument is null or empty. Provide an argument that is not null or empty, and then try the command again.
New-AzRoleAssignment -ObjectId $functionAppSPAppId -RoleDefinitionName "Owner" `
-Scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.DocumentDb/databaseAccounts/$cosmosDbName"


###########################################################
# Assign Azure Function MSI to access ADF
###########################################################

# ERROR: This does not work when using a service principal to connect to Azure.  It says object id is ''
# Cannot validate argument on parameter 'ObjectId'. The argument is null or empty. Provide an argument that is not null or empty, and then try the command again.
New-AzRoleAssignment -ObjectId $functionAppSPAppId -RoleDefinitionName "Contributor" `
-Scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.DataFactory/factories/$dataFactoryName"
