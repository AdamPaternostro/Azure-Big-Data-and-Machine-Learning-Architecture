using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace AzureFunctionApp
{
    public static class GetAzureStorageSASUploadToken
    {
        [FunctionName("GetAzureStorageSASUploadToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("GetAzureStorageSASUploadToken begin request.");

                string cloudAccountName = null;
                string cloudKey = null;


                if (req.Host.ToString().ToLower().Contains("localhost"))
                {
                    cloudAccountName = "devstoreaccount1";
                    cloudKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
                }
                else
                {
                    // Uses MSI to get an Azure AD token: You can run locally if you have a domain joined computer and your domain is synced with Azure AD
                    // The Function App must be in a Policy to to read secrets
                    AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                    KeyVaultClient keyvaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                    string keyVault = Environment.GetEnvironmentVariable("MY_KEY_VAULT");

                    var secretCloudAccountName = await keyvaultClient.GetSecretAsync($"https://{keyVault}.vault.azure.net/", "LandingZoneStorageAccountName");
                    var secretcloudKey = await keyvaultClient.GetSecretAsync($"https://{keyVault}.vault.azure.net/", "LandingZoneStorageAccountKey");

                    cloudAccountName = secretCloudAccountName.Value;
                    cloudKey = secretcloudKey.Value;
                }


                string customerId = req.Query["customerId"];
                string customerSecret = req.Query["customerSecret"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                customerId = customerId ?? data?.customerId;
                customerSecret = customerSecret ?? data?.customerSecret;
                customerId = customerId.ToLower();

                DocumentDBRepository<CosmosIngestionData> documentDBRepository = new DocumentDBRepository<CosmosIngestionData>(log);
                var result = documentDBRepository.GetItems(o => o.CustomerId == customerId && 
                                                           o.PartitionId == customerId && 
                                                           o.CustomerSecret == customerSecret &&
                                                           o.isCustomerEnabled).FirstOrDefault();


                // INSERT SEED DATA
                if (result == null && customerId == "acmeinc")
                {
                    CosmosIngestionData acmeInc = new CosmosIngestionData();
                    acmeInc.CustomerId = "acmeinc";
                    acmeInc.PartitionId = "acmeinc";
                    acmeInc.ContainerName = "acmeinc";
                    acmeInc.CustomerSecret = "0DC8B9026ECD402C84C66AFB5B87E28C";
                    acmeInc.CustomerSASTokenExpireTimeInMinutes = 60;
                    acmeInc.CustomerWhitelistIPAddress = null;
                    acmeInc.isCustomerEnabled = true;
                    acmeInc.ADFPipelineName = "CopyLandingDataToDataLake";
                    acmeInc.ADFResourceGroup = Environment.GetEnvironmentVariable("ResourceGroup");
                    acmeInc.ADFDataFactoryName = Environment.GetEnvironmentVariable("DataFactoryName");
                    acmeInc.ADFSubscriptionId = Environment.GetEnvironmentVariable("SubscriptionId");
                    acmeInc.isADFEnabled = true;

                    // Insert the seed data
                    documentDBRepository.Client.UpsertDocumentAsync(documentDBRepository.Collection.SelfLink, acmeInc).Wait();

                    result = acmeInc;
                }


                if (result != null)
                {
                    // create a blob container
                    // create a SAS token with list and write privilages (no read or delete) - they can upload, but never download to protect their data
                    string sasToken = GetSASToken(result.ContainerName, result.CustomerWhitelistIPAddress, result.CustomerSASTokenExpireTimeInMinutes, cloudAccountName, cloudKey);
                    ReturnData returnData = new ReturnData()
                    {
                        AccountName = cloudAccountName == "devstoreaccount1" ? "http://127.0.0.1:10000/devstoreaccount1" : cloudAccountName,
                        ContainerName = result.ContainerName,
                        SASToken = sasToken
                    };
                    return (ActionResult)new OkObjectResult(returnData);
                }
                else
                {
                    return new UnauthorizedResult();
                }

            }
            catch (Exception ex)
            {
                log.LogError("GetAzureStorageSASUploadToken Exception: " + ex.ToString());

                return new BadRequestResult();
            }
        } // Run


        private static string GetSASToken(string containerName, string clientIPAddress, int tokenExpireTimeInMinutes, string cloudAccountName, string cloudKey)
        {

            Microsoft.WindowsAzure.Storage.Auth.StorageCredentials storageCredentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(cloudAccountName, cloudKey);
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = null;

            if (cloudAccountName == "devstoreaccount1")
            {
                storageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(storageCredentials,
                   new Uri("http://127.0.0.1:10000/devstoreaccount1"),
                   new Uri("http://127.0.0.1:10001/devstoreaccount1"),
                   new Uri("http://127.0.0.1:10002/devstoreaccount1"),
                   new Uri("http://127.0.0.1:10003/devstoreaccount1"));
            }
            else
            {
                storageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(storageCredentials, true);
            }

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
            blobStorage.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.LinearRetry(TimeSpan.FromSeconds(1), 10);
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container = blobStorage.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync().Wait();

            Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy policy = new Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy();
            policy.Permissions = Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.Write | Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.List;
            policy.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5); // always do in the past to prevent errors
            policy.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(tokenExpireTimeInMinutes);


            string sasToken = null;
            if (string.IsNullOrWhiteSpace(clientIPAddress) || cloudAccountName == "devstoreaccount1")
            {
                sasToken = container.GetSharedAccessSignature(policy);
            }
            else
            {
                Microsoft.WindowsAzure.Storage.IPAddressOrRange iPAddressOrRange = new Microsoft.WindowsAzure.Storage.IPAddressOrRange(clientIPAddress);
                sasToken = container.GetSharedAccessSignature(policy, null, Microsoft.WindowsAzure.Storage.SharedAccessProtocol.HttpsOnly, iPAddressOrRange);
            }

          //  string url = "https://" + cloudAccountName + ".blob.core.windows.net" + sasToken;

            return sasToken;

        }

   
        private struct ReturnData
        {
            public string AccountName { get; set; }
            public string ContainerName { get; set; }
            public string SASToken { get; set; }
        }

    } // class
} // namespace
