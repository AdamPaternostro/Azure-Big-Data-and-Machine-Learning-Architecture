# Azure-Big-Data-and-Machine-Learning-Architecture
A ready to use architecture for processing data and performing machine learning in Azure


### ARM Templates
- DONE: Storage (Landing)
- Service Bus (or Azure queue)
- DONE: Data Factory
- App Service
  - Azure Function (Call ADF)
  - Azure Function (Customer get SAS token)
  - Azure Function (Process AAS Cube)
 - DONE: CosmosDB
 - DONE: ADLS Gen 2
 - DONE: Databricks
- Machine Learning Services
- DONE: SQL DW
- SQL Hyperscale
- Synapse Workspace (preview pending)
- Analysis Services

### Streaming
- Event Hub
- Sample database with CDC

### Secure it
- VNET

### Coding
- Azure Function that processes service bus messages for batch ingestion (Adam)
- Azure Function that processes the AAS cube (Jeremy)
- Azure Function get SAS token

### Wiring
- ADF to GitHub
-

### Azure DevOps
- Multistage templates

### Samples
- Download sample data
- Sample generator program to generate streaming data for streaming pattern
- Sample databricks notebooks for procssing
- Sample Data flows for processing
- Sample data wragling for processing
- Load SQL DW using ADF
- SQL DW / SQL Server create tables (DACPAC)
- Create Hive Tables
- Process ML model
- FTP would be great, but a lot of work
- AzCopy use of customer uploading files

Service Principle
- Create
- Key Vault? for ARM passwords, we would need to deploy this first (or at least have the secrets placed in it)
