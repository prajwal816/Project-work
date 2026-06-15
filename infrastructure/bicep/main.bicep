// ============================================================================
// BazaarVoice Loyalty Integration — Main Bicep Template
// Deploys all Azure resources for the integration pipeline.
// Usage: az deployment group create -g <rg-name> -f main.bicep -p @parameters/dev.parameters.json
// ============================================================================

@description('Environment name (dev, uat, prod)')
@allowed([
  'dev'
  'uat'
  'prod'
])
param environment string

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Base name prefix for resources')
param projectName string = 'bazaarvoice'

// ── Module: Storage Account ──────────────────────────────────────────────────
module storage 'modules/storage.bicep' = {
  name: 'storage-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
  }
}

// ── Module: Application Insights ─────────────────────────────────────────────
module appInsights 'modules/appinsights.bicep' = {
  name: 'appinsights-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
  }
}

// ── Module: Key Vault ────────────────────────────────────────────────────────
module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
  }
}

// ── Module: Service Bus ──────────────────────────────────────────────────────
module serviceBus 'modules/servicebus.bicep' = {
  name: 'servicebus-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
  }
}

// ── Module: Function Apps ────────────────────────────────────────────────────
module functionApps 'modules/functionapp.bicep' = {
  name: 'functionapp-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    storageAccountName: storage.outputs.storageAccountName
    storageAccountConnectionString: storage.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    appInsightsConnectionString: appInsights.outputs.connectionString
    serviceBusConnectionString: serviceBus.outputs.connectionString
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────
output storageAccountName string = storage.outputs.storageAccountName
output blobProcessorFunctionAppName string = functionApps.outputs.blobProcessorName
output messageProcessorFunctionAppName string = functionApps.outputs.messageProcessorName
output serviceBusNamespace string = serviceBus.outputs.namespaceName
output appInsightsName string = appInsights.outputs.appInsightsName
output keyVaultName string = keyVault.outputs.keyVaultName
