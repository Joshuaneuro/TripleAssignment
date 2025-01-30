param location string = resourceGroup().location

var prefix = uniqueString('TripleAzure', location, resourceGroup().name, subscription().subscriptionId) 
var serverFarmName = '${prefix}sf'
var functionAppName = 'TripleAzure'
var storageAccountName = '${prefix}sta'

@description('Name of the storage queue')
param queueName string = 'start-weather-job'

@description('Name of the storage queue')
param queueName2 string = 'check-weather-job'



resource serverFarm 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: serverFarmName
  location: location
  tags: resourceGroup().tags
  sku: {
    tier: 'Consumption'
    name: 'Y1'
  }
  kind: 'elastic'
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  tags: resourceGroup().tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: serverFarm.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ]
    }
    clientAffinityEnabled: false
    httpsOnly: true
    containerSize: 1536
    redundancyMode: 'None'
  }
  dependsOn: [
    storageAccount
    serverFarm
  ]

  resource functionAppConfig 'config@2021-03-01' = {
    name: 'appsettings'
    properties: {
        // function app settings
        FUNCTIONS_EXTENSION_VERSION: '~4'
        FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
        WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
        AzureWebJobsStorage: storageAccountConnectionString
        WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
        WEBSITE_CONTENTSHARE: toLower(functionAppName)
      }
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: resourceGroup().tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: true
    minimumTlsVersion: 'TLS1_2'
    accessTier: 'Hot'
    publicNetworkAccess: 'Enabled'
  }
}

resource storageQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = {
  name: '${storageAccount.name}/default/${queueName}'
}
resource storageQueue2 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = {
  name: '${storageAccount.name}/default/${queueName2}'
}