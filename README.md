# .NET Middleware library for Aserto

[![ci](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml/badge.svg)](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml) [![Coverage Status](https://coveralls.io/repos/github/aserto-dev/aserto-dotnet/badge.svg?branch=main&t=1UzNg5)](https://coveralls.io/github/aserto-dev/aserto-dotnet?branch=main) [![NuGet version](https://img.shields.io/nuget/v/Aserto.AspNetCore.Middleware?style=flat)](https://www.nuget.org/packages/Aserto.AspNetCore.Middleware/)

Aserto.AspNetCore.Middleware is a middleware that allows .NET Asp applications to use Aserto as the Authorization provider.

## Prerequisites
* [.NET SDK](https://dotnet.microsoft.com/download) 3.1 or newer.
* An [Aserto](https://console.aserto.com) account.

## Installation
[Aserto.AspNetCore.Middleware](https://www.nuget.org/packages/Aserto.AspNetCore.Middleware/) is provided as a NuGet package. 

It can be installed:
* Using Package Manager:
```powershell
Install-Package Aserto.AspNetCore.Middleware
```

 * Using .NET CLI
```sh
dotnet add package Aserto.AspNetCore.Middleware
```

## Configuration
The following configuration settings are required for Aserto.AspNetCore middleware. You can add them to your `appsettings.json`:
```json
// appsettings.json

"Aserto": {
    "AuthorizerApiKey": "YOUR_AUTHORIZER_API_KEY",
    "TenantID": "YOUT_ASERTO_TENANTID",
    "PolicyID": "YOUR_ASERTO_POLICY_ID",
    "PolicyRoot": "YOUR_POLICY_ROOT"
}
``` 
This settings can be retrieved from the [Policy Settings](https://console.aserto.com/ui/policies) page of your Aserto account.

The middleware accepts the following optional parameters:

| Parameter name | Default value | Description |
| -------------- | ------------- | ----------- |
| Enabled | true | Enables or disables Aserto Authorization |
| ServiceUrl | "https://authorizer.prod.aserto.com:8443" | Sets the URL for the authorizer endpoint. |
| Decision | "allowed" | The decision that will be used by the middleware when creating an authorizer request. |


## Usage
To configure Aserto Authorization, the Aserto Authorization Service needs to be added to the `ConfigureServices` method in `Startup.cs`

```csharp
// Startup.cs

public void ConfigureServices(IServiceCollection services)
{
   //..

   // Adds the Aserto Authorization service
   services.AddAsertoAuthorization(Configuration.GetSection("Aserto"));
 
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