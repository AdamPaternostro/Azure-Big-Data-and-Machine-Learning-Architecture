###########################################################
# This is a sample Powershell script an end customer can use to upload data to the landing zone
# The customer will need to install PowerShell
# Note: This does not work with the local storage emulator
###########################################################


###########################################################
# Variables
###########################################################
$azureFunctionUrl="https://functionapp00005.azurewebsites.net"
$customerId="AcmeInc"
$customerSecret="0DC8B9026ECD402C84C66AFB5B87E28C"
$azureFunctionCode="baBqKrKC97HA/sLvZvjHtxCq82a43UmevfNSOwJU9DSuUXt6dUAixA=="
$today=(Get-Date).ToString('yyyy-MM-dd')


###########################################################
# Call to get SAS token
###########################################################
$json=(Invoke-restmethod -Uri "$azureFunctionUrl/api/GetAzureStorageSASUploadToken?code=$azureFunctionCode&customerId=$customerId&customerSecret=$customerSecret")


###########################################################
# Parse the returned values
###########################################################
$accountName=$json.accountName
$containerName=$json.containerName
$sasToken=$json.sasToken

Write-Output "Account Name:   $accountName"
Write-Output "Container Name: $containerName"
Write-Output "SAS Token:      $sasToken"


###########################################################
# Create a Test Sample file to upload
###########################################################
Write-Output "CustomerId,CustomerName" > myFile.csv
Write-Output "1,Microsoft" >> myFile.csv
Write-Output "2,Contoso"   >> myFile.csv
Write-Output "3,Acme"      >> myFile.csv
Write-Output "4,Walmart"   >> myFile.csv
Write-Output "5,Target"    >> myFile.csv


###########################################################
# Upload one or many files
#
# NOTES: 
# 1. You could use Azure PowerShell
# 2. You could use Azure CLI
# 3. You could use Azure azcopy commands
###########################################################

# Target path (replace myFile.csv with your file name)
$uri = "https://$accountName.blob.core.windows.net/$containerName/inbox/$today/myFile.csv$sasToken"

$headers = @{
    'x-ms-blob-type' = 'BlockBlob'
}

# Upload file using just REST
Invoke-RestMethod -Uri $uri -Method Put -Headers $headers -InFile myFile.csv


###########################################################
# Upload the signal file that we are done uploading
# Single to Azure we are done by putting a marker complete file
# The file MUST end in "end_file.txt"
###########################################################

# Create a marker file
Write-Output "" > end_file.txt

# Target path
$uri = "https://$accountName.blob.core.windows.net/$containerName/inbox/$today/end_file.txt$sasToken"

$headers = @{
    'x-ms-blob-type' = 'BlockBlob'
}

# Upload file using just REST
Invoke-RestMethod -Uri $uri -Method Put -Headers $headers -InFile end_file.txt
