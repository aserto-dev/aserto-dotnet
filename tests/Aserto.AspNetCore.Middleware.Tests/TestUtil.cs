using Aserto.AspNetCore.Middleware.Tests.Testing;
using Aserto.AspNetCore.Middleware.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Aserto.AspNetCore.Middleware.Policies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Clients.Options;
using Aserto.Clients.Authorizer;

namespace Aserto.AspNetCore.Middleware.Tests
{
    public static class TestUtil
    {
        internal static Action<AsertoOptions> GetValidAsertoConfig()
        {
            Action<AsertoOptions> options = new Action<AsertoOptions>(o =>
            {
                o.PolicyName = "YOUR_POLICY_NAME";
                o.PolicyRoot = "pr";
                o.PolicyPathMapper = Aserto.AspNetCore.Middleware.Options.Defaults.DefaultPolicyPathMapper;
                o.IdentityMapper = Aserto.AspNetCore.Middleware.Options.Defaults.DefaultIdentityContext;
                o.ResourceMapper = Aserto.AspNetCore.Middleware.Options.Defaults.DefaultResourceMapper;
            });
            return options;
        }

        internal static Action<AsertoAuthorizerOptions> GetValidAuthorizerConfig()
        {
            Action<AsertoAuthorizerOptions> options = new Action<AsertoAuthorizerOptions>(o =>
            {
                o.ServiceUrl = "https://testserver.com";
                o.AuthorizerApiKey = "YOUR_AUTHORIZER_API_KEY";
                o.TenantID = "YOUR_TENANT_ID";                
            });
            return options;
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizerAPIClient)
        {
            return GetPolicyWebHostBuilder(testAuthorizerAPIClient, TestUtil.GetValidAsertoConfig(), TestUtil.GetValidAuthorizerConfig());
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizerAPIClient, string uri)
        {
            return GetPolicyWebHostBuilder(testAuthorizerAPIClient, TestUtil.GetValidAsertoConfig(), TestUtil.GetValidAuthorizerConfig(), uri);
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizerAPIClient, Action<AsertoOptions> asertoOptions, Action<AsertoAuthorizerOptions> authzOptions)
        {
            return GetPolicyWebHostBuilder(testAuthorizerAPIClient, asertoOptions, authzOptions, "/foo");
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizationAPIClient, Action<AsertoOptions> options, Action<AsertoAuthorizerOptions> authzOpts, string uri)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(options, authzOpts);
                    TestAuthorizerAPIClient t = new TestAuthorizerAPIClient();
                    services.AddSingleton<IAuthorizerAPIClient, TestAuthorizerAPIClient>(t =>
                    {
                        return testAuthorizationAPIClient;
                    });
                    
                    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                    {
                        options.Authority = "https://domain";
                        options.Audience = "https://audience";
                    });
                    services.AddControllers();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAsertoAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.Map(uri, async context =>
                        {
                            await context.Response.WriteAsync("Hello World");
                        }).Add(endpointBuilder => endpointBuilder.Metadata.Add(new AsertoAttribute()));
                    });
                });
            return builder;
        }
    }
}
