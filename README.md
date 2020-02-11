# Azure-Big-Data-and-Machine-Learning-Architecture
A ready to use architecture for processing data and performing machine learning in Azure


### ARM Templates
- DONE: Storage (landing, machine learning, functionapp)
- DONE: Data Factory
- DONE App Service
    - Azure Function (Call ADF)
    - Azure Function (Customer get SAS token)
    - Azure Function (Process AAS Cube)
- DONE: CosmosDB
- DONE: ADLS Gen 2
- DONE: Databricks
- DONE: Machine Learning Services
- DONE: App Insights (app service and ml workspace)
- DONE: Key Vault
- DONE: Container Registry (for machine learning)
- DONE: SQL DW
- DONE: Analysis Services

- SQL Hyperscale
- Synapse Workspace (preview?)
- Log Analytics
- ADC (preview?)
- Service Bus (or Azure queue)
- Event Hub (if we do a straming same for CDC from a database)


### Secure it
- VNET (We shoudl just have a global parameter that tucks this under a VNET?)


### Coding
- Azure Function that processes service bus messages for batch ingestion (Adam)
- Azure Function that processes the AAS cube (Jeremy)
- Azure Function get SAS token

### Wiring
- ADF to GitHub (we need an auth key so this might be best done by a person?)
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
