# Azure Functions on Azure Container Apps OpenAI Assistants Sample


An end to end demo that demonstrates how to use the Azure Functions OpenAI Assistants triggers hosted as microservices on Azure Functions on Azure Container Apps

## Features

This project framework provides the following features:

* [OpenAI Extension for Azure Functions](https://github.com/Azure/azure-functions-openai-extension)
* [Dapr Extension for Azure Functions](https://github.com/Azure/azure-functions-dapr-extension)
* [Azure Functions on Azure Container Apps hosting](https://github.com/Azure/azure-functions-on-container-apps)
* [Deploy using azd](https://aka.ms/azure-dev/install)

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

```
For the target location, the regions that currently support the models used in this sample are East US or South Central US. For an up-to-date list of regions and models,
check [here](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/concepts/models).  Make sure that all the intended services for this deployment have availability in your targeted regions.

3\. Execute the following command, if you don't have any pre-existing Azure services and want to start from a fresh deployment.

Run azd up - This will provision Azure resources and deploy this sample to those resources

> NOTE: It may take a minute for the application to be fully deployed.
>  GoTo cd ./scripts/deploy.sh to deploy the Static Web Apps  and Function Apps 

## Deploy of Static Web apps and Function apps post infra creation

4\. Deploy SWA front end code to the static webapps service instance created during azd up using swa cli
```sh
swa cli - Provide the prompted inputs
SWA_DEPLOYMENT_TOKEN=$(az staticwebapp secrets list --name $AZURE_STATICWEBSITE_NAME --query "properties.apiKey" --output tsv)

  swa deploy --env production --deployment-token $SWA_DEPLOYMENT_TOKEN
```
5\. Deploy Function app backend image to Azure Functions on Azure Container Apps resource created during azd up 

```sh

cd ../backend
```

6\. Create the function app to host ToDoManager app in the same container app environment 
```sh
az functionapp create --resource-group <azd_created_ResourceGroup> --name <functionapp_name> \
--environment <azd_created_MyContainerappEnvironment> \
--storage-account <Storage_name> \
--functions-version 4 \
--runtime dotnet-isolated \
--image <DOCKER_ID>/<ToDoManager_image_name>:<version>
--workload-profile-name  <WORKLOAD_PROFILE_NAME> \
 --cpu <vcpus> \
 --memory <memory> \
--enable-dapr true \
--dapr-app-id <app-id-name> \
--dapr-app-port <port_number> \
--min-replicas 1
```

> The above function app creates below functions
> ![image](https://github.com/Azure-Samples/Azure-Functions-on-ACA-OpenAI-Assistants-Sample/assets/45637559/034d31c3-a026-484e-87a6-901d5e0e9b94)
> Note down the url of GetTodoManager for eg: https://tododaprfuncapp.xxxxxxxxxxxxxxx-xxxxxxxxxxx.westcentralus.azurecontainerapps.io/api/GetTodoManager?
> Make sure to turn on Dapr for this function app Overview>Settings>Dapr(NEW)> click Enable > Provide App id and App Port as Dapr service invocation trigger is being used for AddToDoManager function
![image](https://github.com/Azure-Samples/Azure-Functions-on-ACA-OpenAI-Assistants-Sample/assets/45637559/f097c1ee-0ea7-468a-82cd-cbf69c836a86)

7\.  Add the GetToDoManager http url To AssistantSkills.cs within the GetTodos and SendEmail functions. Placeholders for this already provided for you in the code
cd ../backend and find the AssistantSkills.cs code to update the above urls

## Create Azure communication service

8\. Create Azure Email communication service using this (tutorial)[https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/create-email-communication-resource?pivots=platform-azp] and add Azure Managed domain.
![image](https://github.com/Azure-Samples/Azure-Functions-on-ACA-OpenAI-Assistants-Sample/assets/45637559/af6a4ad7-ecd0-4b65-ae1d-fc97c6d13736)

After successful completion of above step you may try sending the email from Azure communication service
![image](https://github.com/Azure-Samples/Azure-Functions-on-ACA-OpenAI-Assistants-Sample/assets/45637559/e53a6281-1863-470a-9ccc-c8b45cdb155c)

9\. Update Communication service configs in AssistantSkills.cs within the SendEmail function placeholders have been created for you.
![image](https://github.com/Azure-Samples/Azure-Functions-on-ACA-OpenAI-Assistants-Sample/assets/45637559/72edb7da-c039-478a-aa1f-8eeb51443361)

## Update the dapr app id for ToDoManager Function
Update the App id under the AddTodo placeholder has been created already

![image](https://github.com/Azure-Samples/Azure-Functions-on-ACA-OpenAI-Assistants-Sample/assets/45637559/cf357f04-9585-4e28-b197-1ff674c8ed27)


10\. Package the AssistantSkills function app into a containerized image

```sh
- cd ../backend
- docker build --platform linux  --tag <docker id | acr id>/<image_name>:v1 .
- docker push  <docker id | acr id>/<image_name>:v1
```

11\. Update the Function app created during azd deploy either by using portal or CLI with the backend function image and enable dapr

```sh
az functionapp config container set --name <azd-created-function-app-name> \
--resource-group <azd-created-resource-group-name> \
--image <docker id | acr id>/<image_name>:v1 \
--dapr-enable true
```

12\. Linking from Static Webapps to Functions on Azure Container Apps is not supported so api.ts under /app/frontend/src/app.ts has to be updated function on azure container apps urls. 
Redploy the statis webapps post updating api.ts

```sh
swa cli - Provide the prompted inputs
SWA_DEPLOYMENT_TOKEN=$(az staticwebapp secrets list --name $AZURE_STATICWEBSITE_NAME --query "properties.apiKey" --output tsv)

  swa deploy --env production --deployment-token $SWA_DEPLOYMENT_TOKEN
```

13\. Update CORs with static webapps url to allow the frontend app to communicate with functions
## Demo

click on the static webapps service url to access the chat application
> Go head and create tasks
> For eg: create a task to book an appointment at 8 PM
> create a task to collect reports before appointment
> list tasks
> send email

![image](https://github.com/Azure-Samples/Azure-Functions-on-ACA-OpenAI-Assistants-Sample/assets/45637559/9011c3dc-df2a-4909-a121-277d5ee67496)



## Resources

[Functions OpenAI demo](https://github.com/Azure-Samples/Azure-Functions-OpenAI-Demo/)

# Getting Started with Azure Functions on Azure Container Apps

Deploy your apps to Azure Functions for cloud-native microservices today!  

Refer to below resources to learn about current and new updates /enhancements to the service.  

Visit the [Getting Started](https://learn.microsoft.com/azure/azure-functions/functions-container-apps-hosting) guide on Microsoft Docs.  

Learn more about pricing details from the  [pricing page](https://aka.ms/containerapps/pricing).  

Reach us via our GitHub page - [Azure/azure-functions-on-container-apps repo.](https://github.com/Azure/azure-functions-on-container-apps)  

Check out the [samples](https://github.com/Azure/azure-functions-on-container-apps/tree/main/samples)  

Help us improve by taking this short [survey](https://microsoft.qualtrics.com/jfe/form/SV_byFGULLJlKPh9Xw)  

 
