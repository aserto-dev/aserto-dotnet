using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Extensions;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.AspNetCore.Middleware.Tests.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Aserto.AspNetCore.Middleware.Tests
{
    //TODO: needs endpoint configuration in test server
    public class AsertoMiddlewareTests
    {
        [Fact]
        public async Task NullServiceThrows()
        {
            var builder = new WebHostBuilder()
              .ConfigureServices(services =>
              {
                  services = null;
                  services.AddAsertoAuthorization(TestUtil.GetValidAsertoConfig(), TestUtil.GetValidAuthorizerConfig());
              });
            await Assert.ThrowsAsync<ArgumentNullException>(() => new TestServer(builder).SendAsync(_ => { }));
        }

        [Fact]
        public async Task NullConfigThrows()
        {
            Action<AsertoOptions> configuration = null;
            Action<AsertoAuthorizerOptions> authzConfiguration = null;

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(configuration, authzConfiguration);
                });
            await Assert.ThrowsAsync<ArgumentNullException>(() => new TestServer(builder).SendAsync(_ => { }));
        }

        [Fact]
        public async Task MissingEnabledRejects()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton<IAuthorizerAPIClient, TestAuthorizerAPIClient>();
                    services.AddAsertoAuthorization(TestUtil.GetValidAsertoConfig(), TestUtil.GetValidAuthorizerConfig());
                    services.AddControllers();                  
                    
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAsertoAuthorization();
                 
                });
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("/test");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UnauthorizedRejects()
        {
             var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(TestUtil.GetValidAsertoConfig(), TestUtil.GetValidAuthorizerConfig());
                    TestAuthorizerAPIClient t = new TestAuthorizerAPIClient();
                    services.AddSingleton<IAuthorizerAPIClient, TestAuthorizerAPIClient>(t =>
                    {
                        return new TestAuthorizerAPIClient(false);
                    });
                })
                .Configure(app =>
                {
                    app.UseAsertoAuthorization();
                });
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("/test");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AsertoAuthorizationDisabledAllows()
        {
            Action<AsertoOptions> asertoOptions = new Action<AsertoOptions>(o =>
            {
                o.PolicyName = "YOUR_POLICY_NAME";
                o.PolicyRoot = "policyRoot";
                o.Enabled = false;
            });

            Action<AsertoAuthorizerOptions> authzOptions = new Action<AsertoAuthorizerOptions>(o =>
            {
                o.ServiceUrl = "https://testserver.com";
                o.AuthorizerApiKey = "YOUR_AUTHORIZER_API_KEY";
                o.TenantID = "YOUR_TENANT_ID";
            });

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(asertoOptions, authzOptions);
                    TestAuthorizerAPIClient t = new TestAuthorizerAPIClient();
                    services.AddSingleton<IAuthorizerAPIClient, TestAuthorizerAPIClient>(t =>
                    {
                        return new TestAuthorizerAPIClient(false);
                    });
                })
                .Configure(app =>
                {
                    app.UseAsertoAuthorization();
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetAsync("/");
        }

        [Fact]
        public async Task AsertoAllowsAllows()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(TestUtil.GetValidAsertoConfig(), TestUtil.GetValidAuthorizerConfig());
                    TestAuthorizerAPIClient t = new TestAuthorizerAPIClient();
                    services.AddSingleton<IAuthorizerAPIClient, TestAuthorizerAPIClient>(t =>
                    {
                        return new TestAuthorizerAPIClient(true);
                    });
                })
                .Configure(app =>
                {
                    app.UseAsertoAuthorization();
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetAsync("/");
        }
    }
}
