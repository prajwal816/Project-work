<#
.SYNOPSIS
    Deploys the BazaarVoice Loyalty Integration infrastructure using Bicep.

.DESCRIPTION
    This script deploys all Azure resources for the BazaarVoice Loyalty Integration
    using the Bicep templates in the infrastructure/bicep directory.

.PARAMETER Environment
    Target environment: dev, uat, or prod

.PARAMETER ResourceGroupName
    Override the resource group name. Default: rg-bazaarvoice-loyalty-{environment}

.PARAMETER Location
    Azure region. Default: eastus

.EXAMPLE
    .\deploy.ps1 -Environment dev
    .\deploy.ps1 -Environment prod -Location westus2
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "uat", "prod")]
    [string]$Environment,

    [Parameter(Mandatory = $false)]
    [string]$ResourceGroupName = "",

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus"
)

$ErrorActionPreference = "Stop"

# ── Configuration ─────────────────────────────────────────────────────────────
if ([string]::IsNullOrWhiteSpace($ResourceGroupName)) {
    $ResourceGroupName = "rg-bazaarvoice-loyalty-$Environment"
}

$BicepPath = Join-Path $PSScriptRoot "..\bicep\main.bicep"
$ParametersPath = Join-Path $PSScriptRoot "..\bicep\parameters\$Environment.parameters.json"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " BazaarVoice Loyalty Integration Deployment"   -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Environment:    $Environment"
Write-Host "  Resource Group: $ResourceGroupName"
Write-Host "  Location:       $Location"
Write-Host "  Bicep File:     $BicepPath"
Write-Host "  Parameters:     $ParametersPath"
Write-Host ""

# ── Validate prerequisites ────────────────────────────────────────────────────
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check Azure CLI
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI is not installed. Please install it from https://aka.ms/installazurecli"
    exit 1
}

# Check login
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Not logged in to Azure. Running 'az login'..." -ForegroundColor Yellow
    az login
}

Write-Host "Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host "Subscription: $($account.name) ($($account.id))" -ForegroundColor Green

# ── Validate files exist ──────────────────────────────────────────────────────
if (-not (Test-Path $BicepPath)) {
    Write-Error "Bicep file not found: $BicepPath"
    exit 1
}

if (-not (Test-Path $ParametersPath)) {
    Write-Error "Parameters file not found: $ParametersPath"
    exit 1
}

# ── Create Resource Group ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "Creating resource group: $ResourceGroupName..." -ForegroundColor Yellow

az group create `
    --name $ResourceGroupName `
    --location $Location `
    --output none

Write-Host "Resource group created/verified." -ForegroundColor Green

# ── Deploy Bicep Template ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "Deploying Bicep template..." -ForegroundColor Yellow
Write-Host "This may take several minutes..." -ForegroundColor Gray

$deploymentName = "bazaarvoice-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

$result = az deployment group create `
    --name $deploymentName `
    --resource-group $ResourceGroupName `
    --template-file $BicepPath `
    --parameters "@$ParametersPath" `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Deployment failed!"
    exit 1
}

# ── Display Outputs ───────────────────────────────────────────────────────────
Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host " Deployment Succeeded!"                        -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

$outputs = $result.properties.outputs
Write-Host "  Storage Account:        $($outputs.storageAccountName.value)"
Write-Host "  Blob Processor Func:    $($outputs.blobProcessorFunctionAppName.value)"
Write-Host "  Message Processor Func: $($outputs.messageProcessorFunctionAppName.value)"
Write-Host "  Service Bus Namespace:  $($outputs.serviceBusNamespace.value)"
Write-Host "  App Insights:           $($outputs.appInsightsName.value)"
Write-Host "  Key Vault:              $($outputs.keyVaultName.value)"
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Update Key Vault secrets with actual connection strings"
Write-Host "  2. Deploy Function App #1 (Blob Processor) code"
Write-Host "  3. Deploy Function App #2 (Message Processor) code"
Write-Host "  4. Configure APIM endpoints for Common Loyalty API"
Write-Host "  5. Verify Application Insights telemetry"
Write-Host ""
