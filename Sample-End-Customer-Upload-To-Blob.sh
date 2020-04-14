# NOTES:
# This is a sample bash script an end customer can use to upload data to the landing zone
# The customer will need to install "jq" to parse the returned JSON: https://stedolan.github.io/jq/download/

# Note: This does not work with the local storage emulator

# Example returned JSON
# {"accountName":"landingzonestorage00005","containerName":"acmeinc","sasToken":"?sv=2018-03-28&sr=c&sig=uvhhtoqJhr8My5Oe73%2..."}

#azureFunctionUrl="http://localhost:7071"
azureFunctionUrl="https://functionapp00005.azurewebsites.net"
customerId="AcmeInc"
customerSecret="0DC8B9026ECD402C84C66AFB5B87E28C"
code="baBqKrKC97HA/sLvZvjHtxCq82a43UmevfNSOwJU9DSuUXt6dUAixA=="
json=$(curl "$azureFunctionUrl/api/GetAzureStorageSASUploadToken?code=$code&customerId=$customerId&customerSecret=$customerSecret")

# Parse the variables
accountName="$(echo -n $json | jq .accountName --raw-output)"
containerName="$(echo -n $json | jq .containerName --raw-output)"
sasToken="$(echo -n $json | jq .sasToken --raw-output)"

echo "Account Name:   $accountName"
echo "Container Name: $containerName"
echo "SAS Token:      $sasToken"

today=`date +%Y-%m-%d`

# Create test file
echo "test" > myFile.txt

# Upload 1 or more files to a folder with the date
# NOTE: You can also use azcopy
# ./azcopy copy ./myFolder "https://$landingzonestorage00005.blob.core.windows.net/$containerName$sasToken" --recursive
az storage blob upload --container-name $containerName --file ./myFile.txt --name inbox/$today/myFile.txt --account-name $accountName --sas-token $sasToken    

# Single to Azure we are done by putting a marker complete file
# The file MUST end in "end_file.txt"
touch end_file.txt

az storage blob upload --container-name $containerName --file end_file.txt --name inbox/$today/end_file.txt --account-name $accountName --sas-token $sasToken    
