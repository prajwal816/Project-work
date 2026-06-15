// ============================================================================
// Storage Account + Blob Containers + Lifecycle Management Policy
// ============================================================================

@description('Environment name')
param environment string

@description('Azure region')
param location string

@description('Project name prefix')
param projectName string

var storageAccountName = 'st${projectName}${environment}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

// ── Blob Container: bazaarvoice ──────────────────────────────────────────────
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobServices
  name: 'bazaarvoice'
  properties: {
    publicAccess: 'None'
  }
}

// ── Lifecycle Management Policy (7-day archive retention) ────────────────────
resource lifecyclePolicy 'Microsoft.Storage/storageAccounts/managementPolicies@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    policy: {
      rules: [
        {
          name: 'archive-cleanup-7days'
          enabled: true
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'bazaarvoice/loyalty/archive/'
              ]
            }
            actions: {
              baseBlob: {
                delete: {
                  daysAfterModificationGreaterThan: 7
                }
              }
            }
          }
        }
      ]
    }
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
