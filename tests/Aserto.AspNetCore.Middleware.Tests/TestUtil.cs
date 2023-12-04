using Aserto.AspNetCore.Middleware.Clients;
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

namespace Aserto.AspNetCore.Middleware.Tests
{
    public static class TestUtil
    {
        internal static Action<AsertoOptions> GetValidConfig()
        {
            Action<AsertoOptions> options = new Action<AsertoOptions>(o =>
            {
                o.ServiceUrl = "https://testserver.com";
                o.AuthorizerApiKey = "YOUR_AUTHORIZER_API_KEY";
                o.TenantID = "YOUR_TENANT_ID";
                o.PolicyName = "YOUR_POLICY_NAME";
                o.PolicyRoot = "pr";
            });
            return options;
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizerAPIClient)
        {
            return GetPolicyWebHostBuilder(testAuthorizerAPIClient, TestUtil.GetValidConfig());
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizerAPIClient, string uri)
        {
            return GetPolicyWebHostBuilder(testAuthorizerAPIClient, TestUtil.GetValidConfig(), uri);
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizerAPIClient, Action<AsertoOptions> options)
        {
            return GetPolicyWebHostBuilder(testAuthorizerAPIClient, options, "/foo");
        }

        internal static IWebHostBuilder GetPolicyWebHostBuilder(TestAuthorizerAPIClient testAuthorizationAPIClient, Action<AsertoOptions> options, string uri)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(options);
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

                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("Aserto", policy => policy.Requirements.Add(new AsertoDecisionRequirement()));
                    });
                    services.AddControllers();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.Map(uri, async context =>
                        {
                            await context.Response.WriteAsync("Hello World");
                        }).RequireAuthorization("Aserto");
                    });
                });
            return builder;
        }
    }
}
