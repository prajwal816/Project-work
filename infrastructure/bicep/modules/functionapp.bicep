// ============================================================================
// Function App Resources: App Service Plan + 2 Function Apps
// ============================================================================

@description('Environment name')
param environment string

@description('Azure region')
param location string

@description('Project name prefix')
param projectName string

@description('Storage account name for WebJobs runtime')
param storageAccountName string

@description('Storage account connection string')
@secure()
param storageAccountConnectionString string

@description('Application Insights instrumentation key')
param appInsightsInstrumentationKey string

@description('Application Insights connection string')
param appInsightsConnectionString string

@description('Service Bus connection string')
@secure()
param serviceBusConnectionString string

// ── App Service Plan (Consumption) ───────────────────────────────────────────
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: 'asp-${projectName}-${environment}'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: false
  }
}

// ── Function App #1: Blob Processor ──────────────────────────────────────────
resource blobProcessorFunc 'Microsoft.Web/sites@2023-01-01' = {
  name: 'func-${projectName}-blobprocessor-${environment}'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'BazaarVoiceStorageConnection'
          value: storageAccountConnectionString
        }
        {
          name: 'ServiceBusConnection'
          value: serviceBusConnectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageAccountConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: 'func-${projectName}-blobprocessor-${environment}'
        }
      ]
    }
  }
}

// ── Function App #2: Message Processor ───────────────────────────────────────
resource messageProcessorFunc 'Microsoft.Web/sites@2023-01-01' = {
  name: 'func-${projectName}-msgprocessor-${environment}'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'ServiceBusConnection'
          value: serviceBusConnectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageAccountConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: 'func-${projectName}-msgprocessor-${environment}'
        }
        // PLACEHOLDER: Add CommonLoyaltyApiBaseUrl, AnnexCloudApiBaseUrl,
        //              CosmosDbConnectionString, etc. as Key Vault references:
        // {
        //   name: 'CommonLoyaltyApiBaseUrl'
        //   value: '@Microsoft.KeyVault(SecretUri=...)'
        // }
      ]
    }
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────
output blobProcessorName string = blobProcessorFunc.name
output blobProcessorPrincipalId string = blobProcessorFunc.identity.principalId
output messageProcessorName string = messageProcessorFunc.name
output messageProcessorPrincipalId string = messageProcessorFunc.identity.principalId
