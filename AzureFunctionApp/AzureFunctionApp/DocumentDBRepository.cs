using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;

namespace AzureFunctionApp
{
    public class DocumentDBRepository<T>
    {
        internal readonly Microsoft.Extensions.Logging.ILogger log;

        internal DocumentDBRepository(Microsoft.Extensions.Logging.ILogger log)
        {
            this.log = log;
        }

        //Use the Database if it exists, if not create a new Database
        internal Database ReadOrCreateDatabase()
        {
            var db = Client.CreateDatabaseQuery()
                            .Where(d => d.Id == DatabaseId)
                            .AsEnumerable()
                            .FirstOrDefault();

            if (db == null)
            {
                db = Client.CreateDatabaseAsync(new Database { Id = DatabaseId }).Result;
            }

            return db;
        }

        //Use the DocumentCollection if it exists, if not create a new Collection
        internal DocumentCollection ReadOrCreateCollection(string databaseLink)
        {
            var col = Client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(c => c.Id == CollectionId)
                              .AsEnumerable()
                              .FirstOrDefault();

            if (col == null)
            {
                var collectionSpec = new DocumentCollection
                {
                    Id = CollectionId,
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new System.Collections.ObjectModel.Collection<string> { "/PartitionId" },
                        Version = PartitionKeyDefinitionVersion.V1
                    }
                };
                var requestOptions = new RequestOptions { OfferThroughput = 400 };

                collectionSpec.IndexingPolicy.IndexingMode = IndexingMode.Consistent;

                col = Client.CreateDocumentCollectionAsync(databaseLink, collectionSpec, requestOptions).Result;
            }

            return col;
        }

        //Expose the "database" value from configuration as a property for internal use
        private static string databaseId;
        internal string DatabaseId
        {
            get
            {
                if (string.IsNullOrEmpty(databaseId))
                {
                    string documentDBDatabase = Environment.GetEnvironmentVariable("DocumentDBDatabase");
                    databaseId = documentDBDatabase;
                }

                return databaseId;
            }
        }

        //Expose the "collection" value from configuration as a property for internal use
        private static string collectionId;
        internal string CollectionId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionId))
                {
                    string documentDBCollection = Environment.GetEnvironmentVariable("DocumentDBCollection");
                    collectionId = documentDBCollection;
                }

                return collectionId;
            }
        }

        //Use the ReadOrCreateDatabase function to get a reference to the database.
        private static Database database;
        internal Database Database
        {
            get
            {
                if (database == null)
                {
                    database = ReadOrCreateDatabase();
                }

                return database;
            }
        }

        //Use the ReadOrCreateCollection function to get a reference to the collection.
        private static DocumentCollection collection;
        internal DocumentCollection Collection
        {
            get
            {
                if (collection == null)
                {
                    collection = ReadOrCreateCollection(Database.SelfLink);
                }

                return collection;
            }
        }

        //This property establishes a new connection to DocumentDB the first time it is used, 
        //and then reuses this instance for the duration of the application avoiding the
        //overhead of instantiating a new instance of DocumentClient with each request
        private static DocumentClient client;
        internal DocumentClient Client
        {
            get
            {
                if (client == null)
                {
                    // Uses MSI to get an Azure AD token: You can run locally if you have a domain joined computer and your domain is synced with Azure AD
                    // The Function App must be in the Owner Role of RBAC for the CosmosDB account
                    var tokenProvider = new AzureServiceTokenProvider();
                    string accessToken = tokenProvider.GetAccessTokenAsync("https://management.azure.com/").Result;
                    string subscriptionId = Environment.GetEnvironmentVariable("SubscriptionId");
                    string resourceGroup = Environment.GetEnvironmentVariable("ResourceGroup");
                    string documentDBDatabase = Environment.GetEnvironmentVariable("DocumentDBDatabase");

                    // https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/tutorial-windows-vm-access-cosmos-db
                    // Invoke-WebRequest -Uri 'https://management.azure.com/subscriptions/<SUBSCRIPTION-ID>/resourceGroups/<RESOURCE-GROUP>/providers/Microsoft.DocumentDb/databaseAccounts/<COSMOS DB ACCOUNT NAME>/listKeys/?api-version=2016-03-31' -Method POST -Headers @{Authorization="Bearer $ARMToken"}

                    string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.DocumentDb/databaseAccounts/{documentDBDatabase}/listKeys/?api-version=2016-03-31";
                    string responseBody = "";
                    CosmosDBKey cosmosDBKey;


                    using (HttpClient client = new HttpClient())
                    {
                        var buffer = System.Text.Encoding.UTF8.GetBytes("");
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                        using (HttpResponseMessage response = client.PostAsync(url, byteContent).Result)
                        {
                            response.EnsureSuccessStatusCode();
                            responseBody = response.Content.ReadAsStringAsync().Result;

                            JSONService jsonService = new JSONService();
                            cosmosDBKey = jsonService.Deserialize<CosmosDBKey>(responseBody);
                        }
                    }


                    string endpoint = Environment.GetEnvironmentVariable("DocumentDBEndPoint");

                    //  string endpoint = DocumentDBEndPoint;
                    string authKey = cosmosDBKey.primaryMasterKey;
                    Uri endpointUri = new Uri(endpoint);
                    client = new DocumentClient(endpointUri, authKey);
                }

                return client;
            }
        }

        // Query the database
        public IEnumerable<T> GetItems(Expression<Func<T, bool>> predicate)
        {
            return Client.CreateDocumentQuery<T>(Collection.DocumentsLink)
                .Where(predicate)
                .AsEnumerable();
        }

        // Query the database
        public IEnumerable<T> GetItems(string query)
        {
            return Client.CreateDocumentQuery<T>(Collection.DocumentsLink, query).AsEnumerable();
        }

        public class CosmosDBKey
        {
            public string primaryMasterKey { get; set; }
            public string secondaryMasterKey { get; set; }
            public string primaryReadonlyMasterKey { get; set; }
            public string secondaryReadonlyMasterKey { get; set; }
        }


    } // class
} // namespace