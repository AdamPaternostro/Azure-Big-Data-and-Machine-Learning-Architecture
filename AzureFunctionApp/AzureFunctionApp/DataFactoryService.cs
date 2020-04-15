using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Rest;

using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;

namespace AzureFunctionApp
{
    public class DataFactoryService
    {

        public static void StartPipeline(
            string inputFileSystem, string inputFileDirectory, 
            string outputFileSystem, string outputFileDirectory,
            string pipelineName, string resourceGroup, string dataFactoryName, string subscriptionId, Microsoft.Extensions.Logging.ILogger log)
        {
            // Uses MSI to get an Azure AD token: You can run locally if you have a domain joined computer and your domain is synced with Azure AD
            // The Function App must be in the Contributor Role of RBAC for the CosmosDB account
            var tokenProvider = new AzureServiceTokenProvider();
            string accessToken = tokenProvider.GetAccessTokenAsync("https://management.azure.com/").Result;

            ServiceClientCredentials cred = new TokenCredentials(accessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };

            // Create a pipeline run
            Console.WriteLine("Creating pipeline run...");
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "inputFileSystem", inputFileSystem },
                    { "inputFileDirectory", inputFileDirectory },
                    { "outputFileSystem", outputFileSystem },
                    { "outputFileDirectory", outputFileDirectory }
                };

            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters).Result.Body;
            log.LogInformation("Pipeline run ID: " + runResponse.RunId);
        } // StartPipeline

    } // class
} // namespace
