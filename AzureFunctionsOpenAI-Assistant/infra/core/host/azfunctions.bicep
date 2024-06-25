param name string
param location string = resourceGroup().location
param tags object = {}

// Reference Properties
@description('Location for Application Insights')
// param applicationInsightsName string = ''
param appServicePlanId string
param azureOpenaiService string
param appInsightsConnectionString string
//param azureOpenaiServiceKey string
param azureOpenaiChatgptDeployment string
param azureOpenaigptDeployment string
param azureSearchService string
//param azureSearchServiceKey string
param azureSearchIndex string
param azureStorageContainerName string

// Microsoft.Web/sites/config
param allowedOrigins array = []
param appCommandLine string = ''
param autoHealEnabled bool = true
param numberOfWorkers int = -1
param ftpsState string = 'FtpsOnly'
param httpsOnly bool = true

@description('Storage Account type')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
])
param storageAccountType string = 'Standard_LRS'

@description('The language worker runtime to load in the function app.')
@allowed([
  'dotnet', 'dotnetcore', 'dotnet-isolated', 'node', 'python', 'java', 'powershell', 'custom'
])
param runtimeName string
param runtimeVersion string
// Microsoft.Web/sites Properties
param kind string = 'functionapp,linux,container,azurecontainerapps'

var storageAccountName = '${uniqueString(resourceGroup().id)}azfunctions'
var shareName = 'openaifiles'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountType
  }
  kind: 'Storage'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }

  resource fileServices 'fileServices' = {
    name: 'default'

    resource share 'shares' = {
      name: shareName
    }
  }
}

resource azureOpenAiServiceAccount 'Microsoft.CognitiveServices/accounts@2022-12-01' existing = {
  name : azureOpenaiService
}

resource azureSearchServiceAccount 'Microsoft.Search/searchServices@2022-09-01' existing = {
  name : azureSearchService
}



@description('Log Analytics workspace for ACA env')
resource logws 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'logws${name}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

@description('ACA env without vnet')
resource env 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'mef${name}'
  location: 'westcentralus'
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logws.properties.customerId
        sharedKey: logws.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'consumption'
      }
      {
        name: 'd4'
        workloadProfileType: 'D4'
        minimumCount: 1
        maximumCount: 3
      }
    ]
  }
}


resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: 'ai${name}'
  location: 'westcentralus'
  tags: tags
  kind: 'functionapp,linux,container,azurecontainerapps'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: env.id
    siteConfig: {
      linuxFxVersion: '<add_your_backend_function_app_docker_image_here>'
      cors: {
        allowedOrigins: union([ 'https://portal.azure.com', 'https://ms.portal.azure.com' ], allowedOrigins)
      }
      minimumElasticInstanceCount: 1
      functionAppScaleLimit: 5
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: runtimeName
        }
       
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: name
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'AZURE_OPENAI_ENDPOINT'
          value: 'https://${azureOpenaiService}.openai.azure.com/'
        }
        {
          name: 'CHAT_MODEL_DEPLOYMENT_NAME'
          value: azureOpenaiChatgptDeployment
        }
        {
          name: 'EMBEDDING_MODEL_DEPLOYMENT_NAME'
          value: azureOpenaigptDeployment
        }
        {
          name: 'queueConnection'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'SYSTEM_PROMPT'
          value: 'You are a helpful assistant. You are responding to requests from a user about internal emails and documents. You can and should refer to the internal documents to help respond to requests. If a user makes a request thats not covered by the documents provided in the query, you must say that you do not have access to the information and not try and get information from other places besides the documents provided. The following is a list of documents that you can refer to when answering questions. The documents are in the format [filename]: [text] and are separated by newlines. If you answer a question by referencing any of the documents, please cite the document in your answer. For example, if you answer a question by referencing info.txt, you should add "Reference: info.txt" to the end of your answer on a separate line.'
        }
        {
          name: 'AISearchEndpoint'
          value: 'https://${azureSearchService}.search.windows.net'
        }
     
      ]
      
    }
    httpsOnly: httpsOnly
  }
}

output id string = functionApp.id
output identityPrincipalId string = functionApp.identity.principalId
output name string = functionApp.name
