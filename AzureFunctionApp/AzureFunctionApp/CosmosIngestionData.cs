using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionApp
{
    [Serializable()]
    public class CosmosIngestionData
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string CustomerId { get; set; }

        public string CustomerSecret { get; set; }

        public string ContainerName { get; set; }

        public int TokenExpireTimeInMinutes { get; set; }

        public bool isCustomerEnabled { get; set; }

        public string PartitionId { get; set; }

        public string PipelineName { get; set; }

        public string ResourceGroup { get; set; }

        public string DataFactoryName { get; set; }

        public string SubscriptionId { get; set; }

        public bool isADFEnabled { get; set; }

    }
}