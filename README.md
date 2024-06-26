# Azure Functions on Azure Container Apps OpenAI Assistants Sample


An end to end demo that demonstrates how to use the Azure Functions OpenAI Assistants triggers hosted as microservices on Azure Functions on Azure Container Apps

## Features

This project framework provides the following features:

* OpenAI Extension for Azure Functions
* Dapr Extension for Azure Functions
* Azure Functions on Azure Container Apps hosting
* Deploy using azd

## Getting Started

### Prerequisites

- [Azure Developer CLI](https://aka.ms/azure-dev/install)
- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) - Backend Functions app is built using .NET 8
- [Node.js](https://nodejs.org/en/download/) - Frontend is built in TypeScript
- [Git](https://git-scm.com/downloads)
- [Powershell 7+ (pwsh)](https://github.com/powershell/powershell) - For Windows users only.
- Important: Ensure you can run pwsh.exe from a PowerShell command. If this fails, you likely need to upgrade PowerShell.
- [Static Web Apps Cli](https://github.com/Azure/static-web-apps-cli#azure-static-web-apps-cli)
- [Azure Cli](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Clinux%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools)
-   NOTE: Your Azure Account must have Microsoft.Authorization/roleAssignments/write permissions, such as [User Access Administrator](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#user-access-administrator) or [Owner](https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#owner).
- [Docker](https://docs.docker.com/install/)
- [Docker ID](https://hub.docker.com/signup) OR [Azure Container Registry](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal?tabs=azure-cli) (You must have Docker installed locally. Docker provides packages that easily configure Docker on any [Mac], Windows, or Linux system.)



### Installation

### Project Installation

1\. Clone this project
```sh

- cd ToDoManager
- docker build --platform linux  --tag <docker id | acr id>/<image_name>:v1 .
- docker push  <docker id | acr id>/<image_name>:v1

```

 2\. Navigate to the folder AzureFunctionsOpenAI-Assistant

 ``` sh
cd AzureFunctionsOpenAI-Assistant
Run azd login
Run az account set --subscription "<your target subscription>"
Run azd init
For the target location, the regions that currently support the models used in this sample are East US or South Central US. For an up-to-date list of regions and models, check [here](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/concepts/models). Make sure that all the intended services for this deployment have availability in your targeted regions.
```

3\. Execute the following command, if you don't have any pre-existing Azure services and want to start from a fresh deployment.

Run azd up - This will provision Azure resources and deploy this sample to those resources
After the application has been successfully deployed you will see a URL printed to the console. Click that URL to interact with the application in your browser.
> NOTE: It may take a minute for the application to be fully deployed.
> Make sure Static Web Apps  and Function Apps are deployed successfully else GoTo cd ./scripts/deploy.sh

## Manual deploy of Static Web apps and Function apps post infra creation

4\. Deploy SWA front end code to the static webapps service instance created during azd up using swa cli
```sh
swa cli - Provide the prompted inputs
SWA_DEPLOYMENT_TOKEN=$(az staticwebapp secrets list --name $AZURE_STATICWEBSITE_NAME --query "properties.apiKey" --output tsv)

  swa deploy --env production --deployment-token $SWA_DEPLOYMENT_TOKEN
```
5\. Deploy Function app backend image to Azure Functions on Azure Container Apps resource created during azd up 

```sh

cd ../backend

 

### Quickstart
(Add steps to get up and running quickly)

1. git clone [repository clone url]
2. cd [repository name]
3. ...


## Demo

A demo app is included to show how to use the project.

To run the demo, follow these steps:

(Add steps to start up the demo)

1.
2.
3.

## Resources

(Any additional resources or related projects)

- Link to supporting information
- Link to similar sample
- ...
