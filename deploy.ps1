# deploy.ps1
# This script publishes the Azure Functions project, creates resources using Bicep, and deploys the function app.

# Variables
$resourceGroupName = "weatherfuncazurite"
$location = "eastus"  # Replace with your desired Azure region
$bicepTemplate = "./main.bicep"  # Path to your Bicep template
$functionAppName = "TripleAzure"  # Name of the Azure Function App
$projectPath = "./TripleAzure"  # Path to your Azure Functions project
$publishFolder = "./publish"  # Output folder for `dotnet publish`

# Step 1: Publish the Azure Functions project
Write-Host "Publishing Azure Functions project..."
dotnet publish $projectPath -o $publishFolder -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish the Azure Functions project. Exiting."
    exit 1
}

# Step 2: Deploy resources using Bicep
Write-Host "Deploying Azure resources using Bicep..."
az deployment group create --resource-group $resourceGroupName --template-file $bicepTemplate
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to deploy Azure resources using Bicep. Exiting."
    exit 1
}

# Step 3: Deploy the Azure Functions app
Write-Host "Deploying Azure Functions app..."
az functionapp deployment source config-zip --resource-group $resourceGroupName --name $functionAppName --src $publishFolder
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to deploy the Azure Functions app. Exiting."
    exit 1
}

Write-Host "Deployment completed successfully!"