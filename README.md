# Azure-Big-Data-and-Machine-Learning-Architecture
A ready to use architecture for processing data and performing machine learning in Azure

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Big-Data-and-Machine-Learning-Architecture/master/Images/Azure-Big-Data-Architecture.gif)


# What it does
1. Creates all the necessary Azure resources
2. Wires up security between resources
3. Allows you to upload data as thought you are a customer (SAMPLE-End-Customer-Upload-To-Blob.{ps1 or sh})
   1. An event from the upload will trigger a data factory to move data from the landing storage account to the data lake
4. There is a data factory that will download NYC Taxi data (you execute the pipeline ProcessNYCTaxiData by hand)
   1. (This is being worked on!) A Data Flow will move the data from the landing zone on the data lake to the raw "bronze" zone (it will convert the files to parquet)
   2. A Data Flow will move the data from the raw zone to the transformed "silver" zone (it will add reference data)
   3. A Data Flow will move the data from the transformed zone to the enriched "gold" zone (it will place the data in the ready to use format)
   4. A Data Flow will move the data from the enriched/gold zone to the modeled zone (it will place the data in a b-star schema)
       1. SQL OD will load the data from the modeled zone to an Azure Analysis Service cube
       2. A SQL Hyerscale database will be loaded with the modeled data


# How to run

### Prerequisites
1. Install PowerShell: https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-3.7.0
2. Install Visual Studio (to review the code - the goal is to have a devops deployment, right now you publish the Azure Function by hand)


### Running
1. Clone this repo to your local computer (you can fork if you want)
2. Fork the https://github.com/AdamPaternostro/Azure-Big-Data-and-Machine-Learning-Architecture-ADF to a GitHub account
3. Replace the string "00005" with something else in lowercase e.g. "00099" withing all the downloaded files (hint: use VS Code or something).  This will generate unique Azure names.
3. Run STEP-01-CreateResourceGroupAndServicePrinciple.ps1 (must be an Azure admin)
4. Run STEP-02-Deploy-ARM-Template.ps1 (uses service principal above)
5. Run STEP-03-InitializationScript.ps1 (must be an Azure admin, at least until the service principal gets correct permissions set)


### Copy Sample Taxi Data
- Open the data factory
- Authorize Azure to talk to your GitHub
- Run the pipeline: ProcessNYCTaxiData

### Upload Sample Data (as though you are a customer)
- Publish the Azure Function (right click in Visual Studio and click Publish)
- Open the SAMPLE-End-Customer-Upload-To-Blob.{ps1 or sh}
  - Change the Azure Function "code" line: $azureFunctionCode="baBqKrKC97HA/sLvZvjHtxCq82a43UmevfNSOwJU9DSuUXt6dUAixA==".  You get this from the Azure Portal and click on the function GetAzureStorageSASUploadToken and the click the "</> Get function URL" and copy the code.
- Run the sample
  - You should see the script generate a file and upload it
  - An end_file.txt will be generated and uploaded
  - The script will complete
  - A queue in the landing storage account named "fileevent" should get an item in it 
  - The Azure Function will run every 5 minutes and pickup the queue item
  - The Azure Function will kick off the ADF Pipeline CopyLandingDataToDataLake
  - The ADF pipeline will copy the data from the landing storage account to the data lake.


# Task List

### Coding
- Azure Function that processes the AAS cube (Jeremy)


### Azure DevOps
- Multistage templates


### Samples
- Sample generator program to generate streaming data for streaming pattern
- Sample databricks notebooks for procssing
- Sample Data flows for processing
- Sample data wragling for processing
- Load SQL DW using ADF
- SQL DW / SQL Server create tables (DACPAC)
- Create Hive Tables
- Process ML model
- FTP would be good to include


### Security
- Trying to use MSI for everything!
- Create service principle only if needed (so far just one to deploy this)
- Databricks could use Scopes for secrets
- Could use KeyVault for secrets (if so then access using MSI)


## Notes
- create key vault policy via arm


### Adam Tasks
- Moving data
- use SQL OD to load AAS
- AAS needs full SDK for Azure Function v1 (does v1 support durrble functions)
- Use NYC taxi data (over time there is schema drift)
- Sample Data
  - read with Spark (or data flows)
  - do few joins
  - do partitions
  - result: Partitioned data
- Customer could upload data and I can merge into the Sample Data set and process cube
