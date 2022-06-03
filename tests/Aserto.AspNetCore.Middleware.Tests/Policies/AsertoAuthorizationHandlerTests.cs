using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Extensions;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.AspNetCore.Middleware.Policies;
using Aserto.AspNetCore.Middleware.Tests.Testing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aserto.AspNetCore.Middleware.Tests.Policies
{
    public class AsertoAuthorizationHandlerTests
    {
        [Fact]
        public async Task MissingConfigThrows()
        {
            var testConfig = new Dictionary<string, string>
            {
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(testConfig).Build();
            Action<AsertoOptions> options = new Action<AsertoOptions>(o => new AsertoOptions());
            configuration.GetSection("Aserto").Bind(options);

            var t = new TestAuthorizerAPIClient(false);
            var builder = TestUtil.GetPolicyWebHostBuilder(t, options);
            var testServer = new TestServer(builder);

            await Assert.ThrowsAsync<OptionsValidationException>(() => testServer.CreateClient().GetAsync("/foo"));
            
        }

        [Fact]
        public async Task UnauthorizedRejects()
        {
            var t = new TestAuthorizerAPIClient(false);
            var builder = TestUtil.GetPolicyWebHostBuilder(t);
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("/foo");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AsertoAuthorizationDisabledAllows()
        {
            var t = new TestAuthorizerAPIClient(false);

            Action<AsertoOptions> options = new Action<AsertoOptions>(o =>
            {
                o.ServiceUrl = "https://testserver.com";
                o.AuthorizerApiKey = "YOUR_AUTHORIZER_API_KEY";
                o.TenantID = "YOUR_TENANT_ID";
                o.PolicyID = "YOUR_POLICY_ID";
                o.PolicyRoot = "policy_root";
                o.Enabled = false;
            });

            var builder = TestUtil.GetPolicyWebHostBuilder(t, options);
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("/foo");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("tp", "GET", "https://localhost/", "tp.GET")]
        [InlineData("tp", "DELETE", "https://localhost/", "tp.DELETE")]
        [InlineData("tp", "PATCH", "https://localhost/", "tp.PATCH")]
        [InlineData("tp", "PUT", "https://localhost/", "tp.PUT")]
        [InlineData("tp", "POST", "https://localhost/foo", "tp.POST.foo")]
        [InlineData("tp", "GET", "https://localhost/foo", "tp.GET.foo")]
        [InlineData("tp", "GET", "https://localhost/?a=b", "tp.GET")]
        [InlineData("tp", "GET", "https://localhost/en-us/api", "tp.GET.en_us.api")]
        [InlineData("tp", "GET", "https://localhost/en-us?view=3", "tp.GET.en_us")]
        [InlineData("tp", "GET", "https://localhost/en_us", "tp.GET.en_us")]
        [InlineData("tp", "GET", "https://localhost/til~de", "tp.GET.til_de")]
        [InlineData("tp", "GET", "https://localhost/__id", "tp.GET.__id")]
        [InlineData("tp", "POST", "https://localhost/__id", "tp.POST.__id")]
        [InlineData("tp", "GET", "https://localhost/v1", "tp.GET.v1")]
        [InlineData("tp", "GET", "https://localhost/dotted.endpoint", "tp.GET.dotted.endpoint")]
        [InlineData("tp", "GET", "https://localhost/a?dotted=q.u.e.r.y", "tp.GET.a")]
        [InlineData("tp", "GET", "https://localhost/nuberic/123456/1", "tp.GET.nuberic.123456.1")]
        [InlineData("tp", "GET", "https://localhost/Upercase", "tp.GET.upercase")]
        [InlineData("tp", "GET", "https://localhost/api/:colons", "tp.GET.api.__colons")]
        [InlineData("tp", "POST", "https://localhost/api/:colons", "tp.POST.api.__colons")]
        [InlineData("tp", "DELETE", "https://localhost/api/:colons", "tp.DELETE.api.__colons")]
        public async Task UriToPolicyPath(string policyRoot, string method, string uri, string expected)
        {
            var t = new TestAuthorizerAPIClient(policyRoot);
            var builder = TestUtil.GetPolicyWebHostBuilder(t, new UriBuilder(uri).Path);
            var testServer = new TestServer(builder);

            var testClient = testServer.CreateClient();
            switch (method)
            {
                case "GET":
                    await testClient.GetAsync(uri);
                    break;
                case "POST":
                    await testClient.PostAsync(uri, new StringContent("a=b"));
                    break;
                case "DELETE":
                    await testClient.DeleteAsync(uri);
                    break;
                case "PUT":
                    await testClient.PutAsync(uri, new StringContent("a=b"));
                    break;
                case "PATCH":
                    await testClient.PatchAsync(uri, new StringContent("a=b"));
                    break;
                default:
                    throw new InvalidOperationException($"{method} is not a valid HTTP request method");
            }
            Assert.Equal(expected, t.IsReq.PolicyContext.Path);
        }

        [Theory]
        [InlineData("tp", "GET", "api/{asset}", "https://localhost/api/:something", "tp.GET.api.__asset")]
        [InlineData("tp", "GET", "api/{asset}", "https://localhost/api/:upperCase", "tp.GET.api.__asset")]
        [InlineData("tp", "POST", "api/{asset}", "https://localhost/api/:something", "tp.POST.api.__asset")]
        [InlineData("tp", "PUT", "api/{asset}", "https://localhost/api/:something", "tp.PUT.api.__asset")]
        [InlineData("tp", "DELETE", "api/{asset}", "https://localhost/api/:something", "tp.DELETE.api.__asset")]
        [InlineData("tp", "GET", "api/{asset1}/{asset2}", "https://localhost/api/:multiple/:value", "tp.GET.api.__asset1.__asset2")]
        [InlineData("tp", "GET", "api/{asset}/not_last", "https://localhost/api/:value/not_last", "tp.GET.api.__asset.not_last")]
        [InlineData("tp", "GET", "api/{asset}/not_last", "https://localhost/api/val/not_last", "tp.GET.api.__asset.not_last")]
        [InlineData("tp", "GET", "api/{asset}", "https://localhost/api/no_colons", "tp.GET.api.__asset")]
        [InlineData("tp", "GET", "api/{asset}/another/{asset2}", "https://localhost/api/api/another/api", "tp.GET.api.__asset.another.__asset2")]
        [InlineData("tp", "GET", "api/{asset}", "https://localhost/api/UperCase", "tp.GET.api.__asset")]
        public async Task RouteValues(string policyRoot, string method, string bindPath, string requestUri, string expected)
        {
            var t = new TestAuthorizerAPIClient(policyRoot);
            var builder = TestUtil.GetPolicyWebHostBuilder(t, bindPath);
            var testServer = new TestServer(builder);

            var testClient = testServer.CreateClient();
            switch (method)
            {
                case "GET":
                    await testClient.GetAsync(requestUri);
                    break;
                case "POST":
                    await testClient.PostAsync(requestUri, new StringContent("a=b"));
                    break;
                case "DELETE":
                    await testClient.DeleteAsync(requestUri);
                    break;
                case "PUT":
                    await testClient.PutAsync(requestUri, new StringContent("a=b"));
                    break;
                case "PATCH":
                    await testClient.PatchAsync(requestUri, new StringContent("a=b"));
                    break;
                default:
                    throw new InvalidOperationException($"{method} is not a valid HTTP request method");
            }
            Assert.Equal(expected, t.IsReq.PolicyContext.Path);
        }
    }
}
