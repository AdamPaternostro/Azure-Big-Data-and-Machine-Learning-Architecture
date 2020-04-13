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
    public static class CustomerFileUpload
    {
        static string cloudAccountName = null;
        static string cloudKey = null;
        static int expireTokenTimeInMinutes = 60; // 1 hour

        [FunctionName("GetAzureStorageSASUploadToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try

            {
                log.LogInformation("GetAzureStorageSASUploadToken begin request.");


                // You should load this data from CosmosDB or a Database
                List<CustomerData> listCustomerData = new List<CustomerData>();
                listCustomerData.Add(new CustomerData() { CustomerId = "AcmeInc", CustomerSecret = "0DC8B9026ECD402C84C66AFB5B87E28C", ContainerName = "acmeinc", Enabled = true });
                listCustomerData.Add(new CustomerData() { CustomerId = "GlobalCorp", CustomerSecret = "001F859AC44D4FEEB8BBA7172D38899C", ContainerName = "globalcorp", Enabled = true });


                // NOT Working - this would only allow this IP to upload the file (you may or may not want this if for some reason a different IP would upload)
                string clientIP = null;
                //clientIP = req.Headers["CLIENT-IP"];

                if (req.Host.ToString().ToLower().Contains("localhost"))
                {
                    cloudAccountName = "devstoreaccount1";
                    cloudKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
                }
                else
                {
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


                if (listCustomerData.Exists(o => o.CustomerId == customerId && o.CustomerSecret == customerSecret))
                {
                    // create a blob container
                    // create a SAS token with list and write privilages (no read or delete) - they can upload, but never download to protect their data
                    CustomerData customerData = listCustomerData.Where(o => o.CustomerId == customerId && o.CustomerSecret == customerSecret).FirstOrDefault();

                    string sasToken = GetSASToken(customerData.ContainerName, clientIP);
                    ReturnData returnData = new ReturnData()
                    {
                        AccountName = cloudAccountName == "devstoreaccount1" ? "http://127.0.0.1:10000/devstoreaccount1" : cloudAccountName,
                        ContainerName = customerData.ContainerName,
                        SASToken = sasToken
                    };
                    return (ActionResult)new OkObjectResult(returnData);
                }
                else
                {
                    return new UnauthorizedResult();
                }
                //    (ActionResult)new OkObjectResult($"Hello, {name}")
            }
            catch (Exception ex)
            {
                log.LogError("GetAzureStorageSASUploadToken Exception: " + ex.ToString());

                return new BadRequestResult();
            }
        } // Run


        private static string GetSASToken(string containerName, string clientIPAddress)
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
            policy.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(expireTokenTimeInMinutes);


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

        private struct CustomerData
        {
            public string CustomerId { get; set; }
            public string CustomerSecret { get; set; }
            public string ContainerName { get; set; }
            public bool Enabled { get; set; }
        }

        private struct ReturnData
        {
            public string AccountName { get; set; }
            public string ContainerName { get; set; }
            public string SASToken { get; set; }
        }

    } // class
} // namespace
