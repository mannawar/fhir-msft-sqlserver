@minLength(1)
param environment string
param appName string = 'fhirapp'
param sqlServerName string = 'SqlServer'
param sqlDatabaseName string = 'fhirDb'
param sqlAdminLogin string = 'mannawar'
param sqlAdminPassword string = 'Zarien@1008'
param sqlServerFhirBaseUrl string = 'fhir-server.azurewebsites.net'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-08-01-preview' = {
  name: '${appName}-plan'
  location: resourceGroup().location
  sku: {
    name: 'B1'
    capacity: 1
  }
}

resource webApp 'Microsoft.Web/sites@2023-08-01-preview' = {
  name: '${appName}-${environment}'
  location: resourceGroup().location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServerName}.database.windows.net,1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'FhirServer__BaseUrl'
          value: 'https://${sqlServerFhirBaseUrl}'
        }
      ]
    }
  }
  dependsOn: [
    appServicePlan
  ]
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: resourceGroup().location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 2
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  name: '${sqlServerName}/${sqlDatabaseName}'
  location: resourceGroup().location
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 2
  }
}

output appServicePlanId string = appServicePlan.id
output webAppUrl string = webApp.properties.defaultHostName
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDatabase.name


//az group create --name fhir-aspnetcore --location westus

//az deployment group create --resource-group fhir-aspnetcore --template-file NetCrudApp\main.bicep --parameters environment='production' appName='fhirapp' sqlServerName='SqlServerwest' sqlDatabaseName='fhirDb' sqlAdminLogin='mannawar' sqlAdminPassword='Zarien@1008' sqlServerFhirBaseUrl='fhir-server.azurewebsites.net'

