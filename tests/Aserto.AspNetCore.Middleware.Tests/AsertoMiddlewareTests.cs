using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Extensions;
using Aserto.AspNetCore.Middleware.Tests.Testing;
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
    public class AsertoMiddlewareTests
    {
        [Fact]
        public async Task NullServiceThrows()
        {
            var builder = new WebHostBuilder()
              .ConfigureServices(services =>
              {
                  services = null;
                  services.AddAsertoAuthorization(TestUtil.GetValidConfig());
              });
            await Assert.ThrowsAsync<ArgumentNullException>(() => new TestServer(builder).SendAsync(_ => { }));
        }

        [Fact]
        public async Task NullConfigThrows()
        {
            ConfigurationSection configuration = null;

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(configuration);
                });
            await Assert.ThrowsAsync<ArgumentNullException>(() => new TestServer(builder).SendAsync(_ => { }));
        }

        [Fact]
        public async Task MissingEnabledRejects()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(TestUtil.GetValidConfig());
                    services.AddSingleton<IAuthorizerAPIClient, TestAuthorizerAPIClient>();
                })
                .Configure(app =>
                {
                    app.UseAsertoAuthorization();
                });
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UnauthorizedRejects()
        {
             var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(TestUtil.GetValidConfig());
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

            var response = await testServer.CreateClient().GetAsync("");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AsertoAuthorizationDisabledAllows()
        {
            var configSection = TestUtil.GetValidConfig();
            configSection["Enabled"] = "false";

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoAuthorization(configSection);
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
                    services.AddAsertoAuthorization(TestUtil.GetValidConfig());
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
