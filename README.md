# Azure-Big-Data-and-Machine-Learning-Architecture
A ready to use architecture for processing data and performing machine learning in Azure

![alt tag](https://raw.githubusercontent.com/AdamPaternostro/Azure-Big-Data-and-Machine-Learning-Architecture/master/Images/Azure-Big-Data-Architecture.gif)

## How to run

### Prerequqites
1. Install PowerShell
2. Install Azure CLI
3. Install Visual Studio (to review the code)

### Running
1. Clone this repo to your local computer (you can fork if you want)
2. Fork the https://github.com/AdamPaternostro/Azure-Big-Data-and-Machine-Learning-Architecture-ADF to a GitHub account
3. Run STEP-01-CreateResourceGroupAndServicePrinciple.ps1
4. Run STEP-02-Deploy-ARM-Template.ps1
5. Run STEP-03-InitializationScript.ps1


## Details

### Cloning
- Fork this Repo to a GitHub account
- Clone your fork to your local computer
- Fork the https://github.com/AdamPaternostro/Azure-Big-Data-and-Machine-Learning-Architecture-ADF to a GitHub account

### Deploying the ARM templates
You should run the templates in the nested folder from your computer.  This is the easiest way to debug.  You can run each step one by one.  The master linked template at the root folder is okay to run once you have run each step by step and verified.

- Follow the Readme in the Nested folder
   - Change each local.parameters.xxxxx.json files
   - Run each step one by one in the Readme

- If running the global template at the root
   - Change the azuredeploy.parameters.json
   - Check everything back into GitHub.  This assumes you have a public repo (if you want to secure your repo you need to implement this: https://github.com/AdamPaternostro/GitHub-Azure-Function-Proxy-for-ARM-Template) 
   - Deploy by using the Readme.md at the root 

### Configure Security
- Run the InitializationScript.ps1 script (you should run step by step by hand)

### Copy Sample Data
- Open the data factory
- Authorize Azure to talk to your GitHub
- Run the pipeline: CopySampleDWDataToDataLake


# Task List
### ARM Templates
- SQL Hyperscale
- Synapse Workspace (preview?)
- Log Analytics
- ADC (preview?)

### Secure it
- VNET (We shoudl just have a global parameter that tucks this under a VNET?)

### Coding
- Azure Function that processes the AAS cube (Jeremy)

### Wiring
- ADF to GitHub (we need an auth key so this might be best done by a person?)

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
- FTP would be great, but a lot of work
- AzCopy use of customer uploading files

### Security
- Try to use MSI for everything!
- Create service principle only if needed
- Databricks could use Scopes for secrets
- Could use KeyVault for secrets (if so then access using MSI)

## Notes
- create blob containers via arm
- create key vault policy via arm
