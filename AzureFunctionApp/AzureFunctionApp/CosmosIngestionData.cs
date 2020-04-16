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

        // Typically the same as the customer id for CosmosDB
        public string PartitionId { get; set; }

        public string CustomerSecret { get; set; }

        public string ContainerName { get; set; }

        // You can also do a range of addresses.  You will need to change this to a begin and end range and then change the SAS token code
        public string CustomerWhitelistIPAddress { get; set; }

        public int CustomerSASTokenExpireTimeInMinutes { get; set; }

        public bool isCustomerEnabled { get; set; }

 
        public string ADFPipelineName { get; set; }

        public string ADFResourceGroup { get; set; }

        public string ADFDataFactoryName { get; set; }

        public string ADFSubscriptionId { get; set; }

        public bool isADFEnabled { get; set; }

    }
}