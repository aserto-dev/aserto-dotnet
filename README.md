# .NET Middleware library for Aserto

[![ci](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml/badge.svg)](https://github.com/aserto-dev/aserto-dotnet/actions/workflows/ci.yaml) [![Coverage Status](https://coveralls.io/repos/github/aserto-dev/aserto-dotnet/badge.svg?branch=main&t=1UzNg5)](https://coveralls.io/github/aserto-dev/aserto-dotnet?branch=main) [![NuGet version](https://img.shields.io/nuget/v/Aserto.AspNetCore.Middleware?style=flat)](https://www.nuget.org/packages/Aserto.AspNetCore.Middleware/)[![Maintainability](https://api.codeclimate.com/v1/badges/8d946af86d3dbd10956b/maintainability)](https://codeclimate.com/github/aserto-dev/aserto-dotnet/maintainability)

Aserto.AspNetCore.Middleware is a middleware that allows .NET Asp applications to use Topaz Authorizer as the Authorization provider.

## Prerequisites
* [.NET SDK](https://dotnet.microsoft.com/download) 3.1 or newer.

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

The middleware accepts the following optional parameters:

| Parameter name | Default value | Description |
| -------------- | ------------- | ----------- |
| Enabled | true | Enables or disables Aserto Authorization |
| ServiceUrl | "https://authorizer.prod.aserto.com:8443" | Sets the URL for the authorizer endpoint. |
| Decision | "allowed" | The decision that will be used by the middleware when creating an authorizer request. |
| AuthorizerApiKey | "" | The authorizer API Key |
| TenantID | "" | Te Aserto Tenant ID |
| Inscure | false | Indicates whether insecure service connections are allowed when using SSL |
| PolicyName | "" | The Aserto policy name |
| PolicyInstanceLabel | "" | instance label that will be used for all the requests |


## Usage
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
      options.PolicyPathMapper = (httpRequest) =>
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
       options.ResourceMapper = (httpRequest) =>
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