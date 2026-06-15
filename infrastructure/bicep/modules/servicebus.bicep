// ============================================================================
// Service Bus Namespace + Topic + Subscription + Dead-Letter Config
// ============================================================================

@description('Environment name')
param environment string

@description('Azure region')
param location string

@description('Project name prefix')
param projectName string

var namespaceName = 'sb-${projectName}-loyalty-${environment}'

// ── Service Bus Namespace ────────────────────────────────────────────────────
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

// ── Topic: BazaarVoice Records ───────────────────────────────────────────────
resource topic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'sbt-bazaarvoice-records'
  properties: {
    defaultMessageTimeToLive: 'P7D'          // 7-day TTL
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enablePartitioning: false
    supportOrdering: true
  }
}

// ── Subscription: Loyalty Processing ─────────────────────────────────────────
resource subscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: topic
  name: 'sbs-bazaarvoice-loyalty'
  properties: {
    lockDuration: 'PT5M'                     // 5-minute lock
    maxDeliveryCount: 10                     // Dead-letter after 10 failed attempts
    defaultMessageTimeToLive: 'P7D'          // 7-day TTL
    deadLetteringOnMessageExpiration: true
    deadLetteringOnFilterEvaluationExceptions: true
    enableBatchedOperations: true
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────
output namespaceName string = serviceBusNamespace.name
output namespaceId string = serviceBusNamespace.id
output topicName string = topic.name
output subscriptionName string = subscription.name
output connectionString string = listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString
