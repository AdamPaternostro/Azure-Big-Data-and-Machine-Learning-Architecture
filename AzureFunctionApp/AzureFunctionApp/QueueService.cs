using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureFunctionApp
{
    public class QueueService
    {
        private const int DEFAULT_SAVE_AND_READ_QUEUE_TIMEOUT_IN_MINUTES = 5;
        private const int DEFAULT_SAVE_QUEUE_RETRY_ATTEMPTS = 4;
        private const int DEFAULT_SAVE_QUEUE_RETRY_WAIT_IN_MILLISECONDS = 200;


        internal QueueService() { }


        /// <summary>
        /// Returns the 1st message as a queue item
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public static Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage GetQueueItem(string queueName, Microsoft.WindowsAzure.Storage.Queue.CloudQueue cloudQueue)
        {
            string containerName = queueName.ToLower(); // must be lower case!
            Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage cloudQueueMessage = cloudQueue.GetMessagesAsync(1).Result.FirstOrDefault();
            return cloudQueueMessage;
        } // GetQueueItem


        /// <summary>
        /// Removes a message from the queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="cloudQueueMessage"></param>
        public static void DeleteQueueItem(string queueName, Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage cloudQueueMessage,
            Microsoft.WindowsAzure.Storage.Queue.CloudQueue cloudQueue)
        {
            string containerName = queueName.ToLower(); // must be lower case!
            cloudQueue.DeleteMessageAsync(cloudQueueMessage);
        } // DeleteQueueItem


        public static void IncreaseQueueItemLock(string queueName, Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage cloudQueueMessage, TimeSpan visibilityTimeout, Microsoft.WindowsAzure.Storage.Queue.CloudQueue cloudQueue)
        {
            string containerName = queueName.ToLower(); // must be lower case!
            cloudQueue.UpdateMessageAsync(cloudQueueMessage, visibilityTimeout, Microsoft.WindowsAzure.Storage.Queue.MessageUpdateFields.Visibility);
        } // DeleteQueueItem



        /// <summary>
        /// Creates the storage and gets a reference (once)
        /// </summary>
        public static Microsoft.WindowsAzure.Storage.Queue.CloudQueue ConnectToQueueStorage(string queueName, string cloudAccountName, string cloudKey)
        {
            try
            {
                Microsoft.WindowsAzure.Storage.Auth.StorageCredentials storageCredentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                   cloudAccountName, cloudKey);

                Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(storageCredentials, true);

                Microsoft.WindowsAzure.Storage.Queue.CloudQueueClient queueStorage = storageAccount.CreateCloudQueueClient();

                queueStorage.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.LinearRetry(TimeSpan.FromSeconds(DEFAULT_SAVE_QUEUE_RETRY_WAIT_IN_MILLISECONDS), DEFAULT_SAVE_QUEUE_RETRY_ATTEMPTS);

                queueStorage.DefaultRequestOptions.ServerTimeout = new TimeSpan(0, DEFAULT_SAVE_AND_READ_QUEUE_TIMEOUT_IN_MINUTES, 0);

                Microsoft.WindowsAzure.Storage.Queue.CloudQueue cloudQueue = queueStorage.GetQueueReference(queueName);
                cloudQueue.CreateIfNotExistsAsync();

                return cloudQueue;
            }
            catch (Exception ex)
            {
                throw new Exception("Storage services initialization failure. "
                    + "Check your storage account configuration settings. If running locally, "
                    + "ensure that the Development Storage service is running. \n"
                    + ex.Message);
            }
        } // ConnectToQueueStorage


    } // class
} // namespace
