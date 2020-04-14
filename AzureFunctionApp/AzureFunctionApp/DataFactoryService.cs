using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Rest;

using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Azure.Services.AppAuthentication;

namespace AzureFunctionApp
{
    public class DataFactoryService
    {
        // These should be in Key Vault / Config
        //const string tenantID = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        //const string applicationId = "8101c23d-add3-4f9d-b884-82f116a73119";
        //const string authenticationKey = "2vNrfJ.pI]coJ8o2YgsFZ3g=wTFGnSe[";
        //const string subscriptionId = "64a14e46-6c7d-4063-9665-c295287ab709";

        //// These should go in the Cosmos DB JSON.  This example just has one ADF in one resource group
        //const string resourceGroup = "BlobTriggerRG";
        //const string dataFactoryName = "BlobTriggerADF";

        public static void StartPipeline(string inputBlobPath, string pipelineName, string resourceGroup, string dataFactoryName, string subscriptionId)
        {
            // Uses MSI to get a token to call to the ADF
            var tokenProvider = new AzureServiceTokenProvider();
            string accessToken =  tokenProvider.GetAccessTokenAsync("https://management.azure.com/").Result;
           // log.Info($"accessToken: {accessToken}");


            // Authenticate and create a data factory management client
           // var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
           // ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
           // AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(accessToken);
            var client = new DataFactoryManagementClient(cred)
            {
                SubscriptionId = subscriptionId
            };

            // Create a pipeline run
            Console.WriteLine("Creating pipeline run...");
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "inputPath", inputBlobPath }
                };
            CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(resourceGroup, dataFactoryName, pipelineName, parameters: parameters).Result.Body;
            Console.WriteLine("Pipeline run ID: " + runResponse.RunId);
        } // StartPipeline

    } // class
} // namespace
