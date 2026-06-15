// ============================================================================
// Key Vault + Secret Placeholders
// ============================================================================

@description('Environment name')
param environment string

@description('Azure region')
param location string

@description('Project name prefix')
param projectName string

var keyVaultName = 'kv-${projectName}-${environment}'

// ── Key Vault ────────────────────────────────────────────────────────────────
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// ── Placeholder Secrets ──────────────────────────────────────────────────────
// PLACEHOLDER: These secrets need to be populated after deployment
// with actual connection strings and API keys.

resource secretStorageConn 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'BazaarVoiceStorageConnection'
  properties: {
    value: 'PLACEHOLDER-UPDATE-AFTER-DEPLOYMENT'
  }
}

resource secretServiceBusConn 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ServiceBusConnection'
  properties: {
    value: 'PLACEHOLDER-UPDATE-AFTER-DEPLOYMENT'
  }
}

resource secretCosmosConn 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'CosmosDbConnectionString'
  properties: {
    value: 'PLACEHOLDER-UPDATE-AFTER-DEPLOYMENT'
  }
}

resource secretApimKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'CommonLoyaltyApiSubscriptionKey'
  properties: {
    value: 'PLACEHOLDER-UPDATE-AFTER-DEPLOYMENT'
  }
}

resource secretAnnexCloudKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AnnexCloudApiKey'
  properties: {
    value: 'PLACEHOLDER-UPDATE-AFTER-DEPLOYMENT'
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────
output keyVaultName string = keyVault.name
output keyVaultId string = keyVault.id
output keyVaultUri string = keyVault.properties.vaultUri
