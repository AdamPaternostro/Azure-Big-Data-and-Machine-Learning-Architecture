using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Linq;

namespace AzureFunctionApp
{
    public static class ProcessLandingDataUsingADF
    {

        [FunctionName("ProcessLandingDataUsingADF")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"ProcessLandingDataUsingADF Timer trigger function executed at: {DateTime.Now}");

                string cloudAccountName = null;
                string cloudKey = null;
                string queueName = "fileevent";

                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyvaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                string keyVault = Environment.GetEnvironmentVariable("MY_KEY_VAULT");

                var secretCloudAccountName = keyvaultClient.GetSecretAsync($"https://{keyVault}.vault.azure.net/", "LandingZoneStorageAccountName");
                var secretcloudKey = keyvaultClient.GetSecretAsync($"https://{keyVault}.vault.azure.net/", "LandingZoneStorageAccountKey");

                cloudAccountName = secretCloudAccountName.Result.Value;
                cloudKey = secretcloudKey.Result.Value;

                Microsoft.WindowsAzure.Storage.Queue.CloudQueue cloudQueue = QueueService.ConnectToQueueStorage(queueName, cloudAccountName, cloudKey);

                Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage queueItem = QueueService.GetQueueItem(queueName, cloudQueue);

                while (queueItem != null)
                {
                    ProcessItem(queueItem, queueName, cloudQueue, log);
                    queueItem = QueueService.GetQueueItem(queueName, cloudQueue);
                }
            }
            catch (Exception ex)
            {
                log.LogError("ProcessLandingDataUsingADF Exception: " + ex.ToString());
            }

        } // Run


        static void ProcessItem(Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage queueItem, string queueName, Microsoft.WindowsAzure.Storage.Queue.CloudQueue cloudQueue, ILogger log)
        {
            log.LogInformation("ProcessItem:" + queueItem);

            JSONService jsonService = new JSONService();

            FileEvent fileEvent = jsonService.Deserialize<FileEvent>(queueItem.AsString);

            log.LogInformation("ProcessItem Topic: " + fileEvent.topic);
            log.LogInformation("ProcessItem Subject: " + fileEvent.subject);

            // Make sure we have the correct event
            if (fileEvent.eventType == "Microsoft.Storage.BlobCreated") // && fileEvent.subject.ToLower().EndsWith("end_file.txt"))
            {
                // e.g. "subject": "/blobServices/default/containers/acmeinc/blobs/inbox/2020-04-13/test_end_file.txt"
                //                                                   01234567890123456789012345678901234567890123456789  14 -> 30 =16
                // e.g. "subject": "/blobServices/default/containers/acmeinc/blobs/test_end_file.txt"
                //                                                   01234567890123456789012345678901234567890123456789  13 -> 13 = 0
                string removedPrefix = fileEvent.subject.ToLower().Replace("/blobservices/default/containers/", string.Empty);
                string customerId = removedPrefix.Substring(0, removedPrefix.IndexOf("/"));
                string partitionId = removedPrefix.Substring(0, removedPrefix.IndexOf("/")); // this just happens to be the same as the customer id in the this example

                // Get the Cosmos DB data as to which pipeline belongs to this customer.
                DocumentDBRepository<CosmosIngestionData> documentDBRepository = new DocumentDBRepository<CosmosIngestionData>(log);
                var result = documentDBRepository.GetItems(o => o.CustomerId == customerId && o.PartitionId == partitionId).FirstOrDefault();

                if (result == null)
                {
                    // Someone forgot to create the CosmosDB document (whoops), send an email and log.  Give the developer "1" hour to get things setup.
                    log.LogInformation("Pipeline does not exist.  Setting queue item to retry in 1 hour.");
                    QueueService.IncreaseQueueItemLock(queueName, queueItem, TimeSpan.FromHours(1), cloudQueue);
                }
                else
                {
                    if (result.isADFEnabled)
                    {
                        // We are good to try to start the ADF
                        log.LogInformation("Starting Pipeline: " + result.ADFPipelineName);

                        try
                        {
                            // Try to start the pipeline (we are "trying" since technically something could have just disabled it :) )
                            DateTime dtNow = DateTime.UtcNow; // you could make this EST or such....
                            string inputFileSystem = customerId;
                            int blobsIndex = removedPrefix.IndexOf("/blobs/") + 7;
                            int lastSlashIndex = removedPrefix.LastIndexOf("/");
                            string inputFileDirectory = removedPrefix.Substring(blobsIndex, lastSlashIndex - blobsIndex);
                            string outputFileSystem = Environment.GetEnvironmentVariable("LandingZoneDataLakeContainer");
                            string outputFileDirectory = string.Format("customer-landing-data/{0}/{1:yyyy}/{2:MM}/{3:dd}", customerId, dtNow, dtNow, dtNow);

                            DataFactoryService.StartPipeline(inputFileSystem, inputFileDirectory, outputFileSystem, outputFileDirectory, result.ADFPipelineName, result.ADFResourceGroup, result.ADFDataFactoryName, result.ADFSubscriptionId, log);
                            QueueService.DeleteQueueItem(queueName, queueItem, cloudQueue);
                        }
                        catch (Exception ex)
                        {
                            // Pipeline could not start (it might be in a "provisioning state", set the queue item to try again in "5" minutes
                            log.LogInformation("Error starting Pipeline (item not dequeued): " + result.ADFPipelineName + " Error: " + ex.ToString());
                            QueueService.IncreaseQueueItemLock(queueName, queueItem, TimeSpan.FromMinutes(5), cloudQueue);
                        }
                    }
                    else
                    {
                        // Pipeline is not enabled, set the queue item to try again in "5" minutes
                        log.LogInformation("Pipeline: " + result.ADFPipelineName + " is not enabled.  Setting queue item to retry in 5 minutes.");
                        QueueService.IncreaseQueueItemLock(queueName, queueItem, TimeSpan.FromMinutes(5), cloudQueue);
                    }
                }
            }
            else
            {
                // Not a valid event item - delete from queue and log (tell someone to fixe the blob event filter)
                QueueService.DeleteQueueItem(queueName, queueItem, cloudQueue);
            }

            System.Threading.Thread.Sleep(500);   // Just so the UI does not scroll too fast

        } // Process Item



    } // class
} // namespace
