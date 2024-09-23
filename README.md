# .NET Client library for Aserto

[![ci](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml/badge.svg)](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml) [![Coverage Status](https://coveralls.io/repos/github/aserto-dev/aserto-dotnet/badge.svg?branch=main&t=1UzNg5)](https://coveralls.io/github/aserto-dev/aserto-dotnet?branch=main) [![NuGet version](https://img.shields.io/nuget/v/Aserto.AspNetCore.Middleware?style=flat)](https://www.nuget.org/packages/Aserto.AspNetCore.Middleware/)[![Maintainability](https://api.codeclimate.com/v1/badges/8d946af86d3dbd10956b/maintainability)](https://codeclimate.com/github/aserto-dev/aserto-dotnet/maintainability)

Aserto.Clients is a library that allows .NET applications to use an Aserto Authorizer and Directory Client.

## Installation
[Aserto.Clients](https://www.nuget.org/packages/Aserto.Clients/) is provided as a NuGet package. 

It can be installed:
* Using Package Manager:
```powershell
Install-Package Aserto.Clients
```

 * Using .NET CLI
```sh
dotnet add package Aserto.Clients
```

## Authorizer Client
A new Authorizer Client can be created as follows:
```csharp
   //Initialize using constructor
   AsertoAuthorizerOptions authzOpts = new AsertoAuthorizerOptions();

   // Set connection details
   authzOpts.AuthorizerApiKey = ConfigurationManager.AppSettings["Authorizer.API.Key"];            
   authzOpts.TenantID = ConfigurationManager.AppSettings["Authorizer.TenantID"];
   authzOpts.ServiceUrl = ConfigurationManager.AppSettings["Authorizer.ServiceURL"];
   authzOpts.Insecure = Convert.ToBoolean(ConfigurationManager.AppSettings["Authorizer.Insecure"]);
                       
   var authorizerOptions = Options.Create(authzOpts);
   var client = new AuthorizerAPIClient(authorizerOptions, new NullLoggerFactory());
```

Example call:
```csharp
 var result = client.ListPoliciesAsync(new ListPoliciesRequest() { PolicyInstance = new PolicyInstance(){
                Name="policy-todo",
                InstanceLabel="policy-todo"
            }
```

## Directory Client
A new Directory Client can be created as follows:
```csharp

   var logggerFactory = new NullLoggerFactory();
   // Initialize options using consttructor.
   var options = new AsertoDirectoryOptions("url_and_port_to_directory_service", "directory_api_key", "directory_tenant_id", false);

   // Intialize optons reading the appsettings.json file.
   var options = new AsertoDirectoryOptions();
   Configuration.GetSection("AsertoDirectory").Bind(options);

   var optionsInt = Microsoft.Extensions.Options.Options.Create(options);
   var directoryClient = new DirectoryAPIClient(optionsInt, logggerFactory);

```
you'll need to provide the directory service URL, an API key and the Tenant ID.
The client can be configure to use SSL connection as insecure by providing `options.Insecure = true;`.

Example call to the directory client:
```csharp

   public async Task GetObject()
   {
      //...

      var directoryClient = new DirectoryAPIClient(optionsInt, logggerFactory);

      // Get an object.
      var getObjectResp = await directoryClient.GetObjectAsync("object_key","object_type");

      // Get the identities for a user.
      var getRelationsResp = await directoryAPI.GetRelationsAsync(subjectType: "user", subjectKey: "userID",relationName: "identifier", relationObjectType: "identity", pageSize: 10);

      //...
   }

```

## Examples

* [Aserto Authorizer Client CLI](https://github.com/aserto-dev/aserto-dotnet/tree/main/examples/AuthorizerClientExample)
* [Directory Client CLI](https://github.com/aserto-dev/aserto-dotnet/tree/main/examples/DirectoryClientExampleCLI)

# .NET Middleware library for Aserto

[![ci](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml/badge.svg)](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml) [![Coverage Status](https://coveralls.io/repos/github/aserto-dev/aserto-dotnet/badge.svg?branch=main&t=1UzNg5)](https://coveralls.io/github/aserto-dev/aserto-dotnet?branch=main) [![NuGet version](https://img.shields.io/nuget/v/Aserto.AspNetCore.Middleware?style=flat)](https://www.nuget.org/packages/Aserto.AspNetCore.Middleware/)[![Maintainability](https://api.codeclimate.com/v1/badges/8d946af86d3dbd10956b/maintainability)](https://codeclimate.com/github/aserto-dev/aserto-dotnet/maintainability)

Aserto.AspNetCore.Middleware is a middleware that allows .NET Asp applications to use Topaz Authorizer as the Authorization provider.

## Prerequisites
* [.NET SDK](https://dotnet.microsoft.com/download)

## Installation
[Aserto.AspNetCore.Middleware](https://www.nuget.org/packages/Aserto.AspNetCore.Middleware/) is provided as a NuGet package. 
[Aserto.Middleware] (https://www.nuget.org/packages/Aserto.Middleware/) is the provided NuGet package that can be used with .Net Framework. 

It can be installed:
* Using Package Manager:
```powershell
Install-Package Aserto.AspNetCore.Middleware
```
or 
```powershell
Install-Package Aserto.Middleware
```

 * Using .NET CLI
```sh
dotnet add package Aserto.AspNetCore.Middleware
``` 
or 
```sh
dotnet add package Aserto.Middleware
```

## Configuration
The following configuration settings are required for Aserto.AspNetCore middleware. You can add them to your `appsettings.json`:
```json
"Aserto": {
    "PolicyRoot": "YOUR_POLICY_ROOT",
}
"AsertoDirectory": {
   "DirectoryTenantID": "DIRECTORY_TENANT_ID",
}
```

The middleware accepts the following optional parameters:

***Aserto section***

| Parameter name | Default value | Description |
| -------------- | ------------- | ----------- |
| Enabled | true | Enables or disables Aserto Authorization |
| ServiceUrl | "https://localhost:8282" | Sets the URL for the authorizer endpoint. |
| Decision | "allowed" | The decision that will be used by the middleware when creating an authorizer request. |
| AuthorizerApiKey | "" | The authorizer API Key |
| TenantID | "" | The Aserto Tenant ID |
| Insecure | false | Indicates whether insecure service connections are allowed when using SSL |
| PolicyName | "" | The Aserto policy name |
| PolicyInstanceLabel | "" | The label of the active policy runtime |

***AsertoDirectory section***

| Parameter name | Default value | Description |
| -------------- | ------------- | ----------- |
| DirectoryInsecure | false | Indicates whether insecure directory service connections are allowed when using SSL |
| DirectoryTenantID | "" | The Aserto Tenant ID of the directory service |
| DirectoryServiceUrl | "https://localhost:9292" | Sets the URL for the directory endpoint. |
| DirectoryApiKey | "" | The directory API Key |


## Usage for Aserto.AspNetCore.Middleware
To configure Aserto Authorization, the Aserto Authorization Service needs to be added to the `ConfigureServices` method in `Startup.cs`

```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
   //..

   // Adds the Aserto Authorization service
   services.AddAsertoAuthorization(options => Configuration.GetSection("Aserto").Bind(options));
 
   //..  
}

```

To use the Authorization, you can now define an Authorization policy with the `AsertoDecisionRequirement` using the following code snippet
```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
   //..

   services.AddAuthorization(options =>
   {
       options.AddPolicy("Aserto", policy => policy.Requirements.Add(new AsertoDecisionRequirement()));
   });

   //..
}
```
To protect your endpoints using Aserto authorization, you need to apply the `[Authorize("Aserto")]` attribute to them.

Using the following code snippet, you can set Aserto authorization as the default Authorization policy. This will enable Aserto Authorization without having to explicitly specify the policy name in the `[Authorize]` attribute.

```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
   //..


   // Use Aserto authorization as the default authorization policy.
   services.AddAuthorization(options =>
   {
       // User is authenticated via a cookie.
       var policy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme);
       policy.AddRequirements(new AsertoDecisionRequirement());
       options.DefaultPolicy = policy.Build();
   });
   
   //..
}
```


### Identity
To determine the identity of the user, the middleware checks the following Claim types:

| Name | Description | URI |
| ---- |------------ |---- |
| E-Mail Address | The e-mail address of the user | http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress |
| Name | The unique name of the user | http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name |
| Name Identifier | The SAML name identifier of the user | http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier |

These can be overwritten by passing other claim types to the `AsertoDecisionRequirement`:

```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
   //..

   services.AddAuthorization(options =>
   {
      options.AddPolicy("Aserto", policy => 
         policy.Requirements.Add(new AsertoDecisionRequirement(new List<string> 
         { 
            "mytype1", 
            "mytype2" 
         })));
   });

   //..
}
```

## URL path to policy mapping
By default, when computing the policy path, the middleware:
* converts all slashes to dots
* converts any character that is not alpha, digit, dot or underscore to underscore
* converts uppercase characters in the URL path to lowercases

This behavior can be overwritten by providing a custom function to the `PolicyPathMapper` AsertoAuthorization option:
```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
   //..

   // Adds the Aserto Authorization service
   services.AddAsertoAuthorization(options =>
   {
      Configuration.GetSection("Aserto").Bind(options));
      options.PolicyPathMapper = (policyRoot, httpRequest) =>
      {
          return "custom.policy.path";
      };
   }
   //..  
}

```

## Resource Mapper
A resource can be any structured data that the authorization policy uses to evaluate decisions. By default, middleware add to the resource context all the route parameters that start with `:`.

Resource data can be overwritten by providing a custom function to the `ResourceMapper` AsertoAuthorization option

```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
   //..

   // Adds the Aserto Authorization service
   services.AddAsertoAuthorization(options =>
     {
       options.ResourceMapper = (policyRoot, httpRequest) =>
       {
         Struct result = new Struct();
         result.Fields["asset"] = Value.ForString("megaSeeds");

         return result;
       };
       Configuration.GetSection("Aserto").Bind(options);
   });
   //..  
}

```

## Directory Client
A new Directory Client can be creating as follows:
```csharp

   var logggerFactory = new NullLoggerFactory();
   // Initialize options using consttructor.
   var options = new AsertoDirectoryOptions("url_and_port_to_directory_service", "directory_api_key", "directory_tenant_id", false);

   // Intialize optons reading the appsettings.json file.
   var options = new AsertoDirectoryOptions();
   Configuration.GetSection("AsertoDirectory").Bind(options);

   var optionsInt = Microsoft.Extensions.Options.Options.Create(options);
   var directoryClient = new DirectoryAPIClient(optionsInt, logggerFactory);

```
you'll need to provide the directory service URL, an API key and the Tenant ID.
The client can be configure to use SSL connection as insecure by providing `options.Insecure = true;`.

Example call to the directory client:
```csharp

   public async Task GetObject()
   {
      //...

      var directoryClient = new DirectoryAPIClient(optionsInt, logggerFactory);

      // Get an object.
      var getObjectResp = await directoryClient.GetObjectAsync("object_key","object_type");

      // Get the identities for a user.
      var getRelationsResp = await directoryAPI.GetRelationsAsync(subjectType: "user", subjectKey: "userID",relationName: "identifier", relationObjectType: "identity", pageSize: 10);

      //...
   }

```


## Building & testing

The project can be built on Windows, Linux or macOS using the [.Net Core SDK](https://dotnet.microsoft.com/download):

```sh
dotnet build .\aserto-dotnet.sln
```

`dotnet` CLI can be used to run the tests from the project:
```sh
dotnet test .\aserto-dotnet.sln
```

## Examples

* [Auth0 authentication and Aserto authorization](https://github.com/aserto-dev/aserto-dotnet/tree/main/examples/Auth0)
* [Duende Identity server for authentication and Aserto authorization](https://github.com/aserto-dev/aserto-dotnet/tree/main/examples/Duende)