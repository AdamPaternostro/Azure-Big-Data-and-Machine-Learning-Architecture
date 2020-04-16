###########################################################
# This is a sample Bash script an end customer can use to upload data to the landing zone
# The customer will need to install "jq" to parse the returned JSON: https://stedolan.github.io/jq/download/
# Note: This does not work with the local storage emulator
###########################################################


###########################################################
# Variables
###########################################################
azureFunctionUrl="https://functionapp00005.azurewebsites.net"
customerId="AcmeInc"
customerSecret="0DC8B9026ECD402C84C66AFB5B87E28C"
azureFunctionazureFunctionCode="baBqKrKC97HA/sLvZvjHtxCq82a43UmevfNSOwJU9DSuUXt6dUAixA=="
today=`date +%Y-%m-%d`


###########################################################
# Call to get SAS token
###########################################################
json=$(curl "$azureFunctionUrl/api/GetAzureStorageSASUploadToken?code=$azureFunctionazureFunctionCode&customerId=$customerId&customerSecret=$customerSecret")


###########################################################
# Parse the returned values
###########################################################
accountName="$(echo -n $json | jq .accountName --raw-output)"
containerName="$(echo -n $json | jq .containerName --raw-output)"
sasToken="$(echo -n $json | jq .sasToken --raw-output)"

echo "Account Name:   $accountName"
echo "Container Name: $containerName"
echo "SAS Token:      $sasToken"



###########################################################
# Create a Test Sample file to upload
###########################################################
echo "CustomerId,CustomerName" > myFile.csv
echo "1,Microsoft" >> myFile.csv
echo "2,Contoso"   >> myFile.csv
echo "3,Acme"      >> myFile.csv
echo "4,Walmart"   >> myFile.csv
echo "5,Target"    >> myFile.csv


###########################################################
# Upload one or many files
#
# NOTES: 
# 1. You could use Azure PowerShell (Core)
# 2. You could use Azure CLI
# 3. You could use Azure azcopy commands
###########################################################
az storage blob upload --container-name $containerName --file ./myFile.csv --name inbox/$today/myFile.csv --account-name $accountName --sas-token $sasToken    


###########################################################
# Upload the signal file that we are done uploading
# Single to Azure we are done by putting a marker complete file
# The file MUST end in "end_file.txt"
###########################################################

# Create a marker file
touch end_file.txt

az storage blob upload --container-name $containerName --file end_file.txt --name inbox/$today/end_file.txt --account-name $accountName --sas-token $sasToken    
