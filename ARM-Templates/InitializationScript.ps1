# NOTE: You might need to wait 5 or 10 minutes after running this script to ensure permissions are proprogated

Connect-AzAccount

# Select Subscription
$subscriptionId="64a14e46-6c7d-4063-9665-c295287ab709"
$context = Get-AzSubscription -SubscriptionId $subscriptionId
Set-AzContext $context

# Script parameters
$resourceGroup="Azure-Big-Data-Machine-Learning"
$dataFactoryName="datafactory00005"
$dataLakeName="datalake00005"

# Configure Managed Idenity for ADF to access the Data Lake

$Identity=(Get-AzDataFactoryV2 -ResourceGroupName $resourceGroup -Name $dataFactoryName).Identity
echo $Identity.PrincipalId

$ServicePrincipal=(Get-AzADServicePrincipal -ObjectId $Identity.PrincipalId)
echo $ServicePrincipal.Id

New-AzRoleAssignment -ObjectId $ServicePrincipal.Id `
    -RoleDefinitionName "Storage Blob Data Contributor" `
    -Scope  "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Storage/storageAccounts/$dataLakeName"