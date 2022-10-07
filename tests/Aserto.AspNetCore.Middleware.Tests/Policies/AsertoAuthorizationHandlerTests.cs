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
        public async Task MissingConfigDoesNotThrows()
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

            await testServer.CreateClient().GetAsync("/foo");

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
                o.PolicyName = "YOUR_POLICY_NAME";
                o.Enabled = false;
            });

            var builder = TestUtil.GetPolicyWebHostBuilder(t, options);
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("/foo");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("GET", "https://localhost/", "GET")]
        [InlineData("DELETE", "https://localhost/", "DELETE")]
        [InlineData("PATCH", "https://localhost/", "PATCH")]
        [InlineData("PUT", "https://localhost/", "PUT")]
        [InlineData("POST", "https://localhost/foo", "POST.foo")]
        [InlineData("GET", "https://localhost/foo", "GET.foo")]
        [InlineData("GET", "https://localhost/?a=b", "GET")]
        [InlineData("GET", "https://localhost/en-us/api", "GET.en_us.api")]
        [InlineData("GET", "https://localhost/en-us?view=3", "GET.en_us")]
        [InlineData("GET", "https://localhost/en_us", "GET.en_us")]
        [InlineData("GET", "https://localhost/til~de", "GET.til_de")]
        [InlineData("GET", "https://localhost/__id", "GET.__id")]
        [InlineData("POST", "https://localhost/__id", "POST.__id")]
        [InlineData("GET", "https://localhost/v1", "GET.v1")]
        [InlineData("GET", "https://localhost/dotted.endpoint", "GET.dotted.endpoint")]
        [InlineData("GET", "https://localhost/a?dotted=q.u.e.r.y", "GET.a")]
        [InlineData("GET", "https://localhost/nuberic/123456/1", "GET.nuberic.123456.1")]
        [InlineData("GET", "https://localhost/Upercase", "GET.Upercase")]
        [InlineData("GET", "https://localhost/api/:colons", "GET.api.__colons")]
        [InlineData("POST", "https://localhost/api/:colons", "POST.api.__colons")]
        [InlineData("DELETE", "https://localhost/api/:colons", "DELETE.api.__colons")]
        public async Task UriToPolicyPath(string method, string uri, string expected)
        {
            var t = new TestAuthorizerAPIClient();
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
        [InlineData("GET", "api/{asset}", "https://localhost/api/:something", "GET.api.__asset")]
        [InlineData("GET", "api/{asset}", "https://localhost/api/:upperCase", "GET.api.__asset")]
        [InlineData("POST", "api/{asset}", "https://localhost/api/:something", "POST.api.__asset")]
        [InlineData("PUT", "api/{asset}", "https://localhost/api/:something", "PUT.api.__asset")]
        [InlineData("DELETE", "api/{asset}", "https://localhost/api/:something", "DELETE.api.__asset")]
        [InlineData("GET", "api/{asset1}/{asset2}", "https://localhost/api/:multiple/:value", "GET.api.__asset1.__asset2")]
        [InlineData("GET", "api/{asset}/not_last", "https://localhost/api/:value/not_last", "GET.api.__asset.not_last")]
        [InlineData("GET", "api/{asset}/not_last", "https://localhost/api/val/not_last", "GET.api.__asset.not_last")]
        [InlineData("GET", "api/{asset}", "https://localhost/api/no_colons", "GET.api.__asset")]
        [InlineData("GET", "api/{asset}/another/{asset2}", "https://localhost/api/api/another/api", "GET.api.__asset.another.__asset2")]
        [InlineData("GET", "api/{asset}", "https://localhost/api/UperCase", "GET.api.__asset")]
        [InlineData("GET", "api/{CaseSensitive}", "https://localhost/api/parameter", "GET.api.__CaseSensitive")]
        public async Task RouteValues(string method, string bindPath, string requestUri, string expected)
        {
            var t = new TestAuthorizerAPIClient();
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
