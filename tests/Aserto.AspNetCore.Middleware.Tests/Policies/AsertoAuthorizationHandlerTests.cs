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
        public async Task UnauthorizedRejects()
        {
            var t = new TestAuthorizerAPIClient(false);
            var builder = TestUtil.GetPolicyWebHostBuilder(t);
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("/foo");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AsertoAuthorizationDisabledAllows()
        {
            var t = new TestAuthorizerAPIClient(false);

            Action<AsertoAuthorizerOptions> options = new Action<AsertoAuthorizerOptions>(o =>
            {
                o.ServiceUrl = "https://testserver.com";
                o.AuthorizerApiKey = "YOUR_AUTHORIZER_API_KEY";
                o.TenantID = "YOUR_TENANT_ID";
            });

            Action<AsertoOptions> asertoOptions = new Action<AsertoOptions>(o =>
            {
                o.PolicyName = "YOUR_POLICY_NAME";
                o.Enabled = false;
                o.PolicyRoot = "policyroot";
            });

            var builder = TestUtil.GetPolicyWebHostBuilder(t, asertoOptions, options);
            var testServer = new TestServer(builder);

            var response = await testServer.CreateClient().GetAsync("/foo");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("pr", "GET", "https://localhost/", "pr.GET")]
        [InlineData("pr","DELETE", "https://localhost/", "pr.DELETE")]
        [InlineData("polroot", "PATCH", "https://localhost/", "polroot.PATCH")]
        [InlineData("pr", "PUT", "https://localhost/", "pr.PUT")]
        [InlineData("pr", "POST", "https://localhost/foo", "pr.POST.foo")]
        [InlineData("pr", "GET", "https://localhost/foo", "pr.GET.foo")]
        [InlineData("pr", "GET", "https://localhost/?a=b", "pr.GET")]
        [InlineData("pr", "GET", "https://localhost/en-us/api", "pr.GET.en_us.api")]
        [InlineData("pr", "GET", "https://localhost/en-us?view=3", "pr.GET.en_us")]
        [InlineData("pr", "GET", "https://localhost/en_us", "pr.GET.en_us")]
        [InlineData("pr", "GET", "https://localhost/til~de", "pr.GET.til_de")]
        [InlineData("pr", "GET", "https://localhost/__id", "pr.GET.__id")]
        [InlineData("pr", "POST", "https://localhost/__id", "pr.POST.__id")]
        [InlineData("pr", "GET", "https://localhost/v1", "pr.GET.v1")]
        [InlineData("pr", "GET", "https://localhost/dotted.endpoint", "pr.GET.dotted.endpoint")]
        [InlineData("pr", "GET", "https://localhost/a?dotted=q.u.e.r.y", "pr.GET.a")]
        [InlineData("pr", "GET", "https://localhost/nuberic/123456/1", "pr.GET.nuberic.123456.1")]
        [InlineData("pr", "GET", "https://localhost/Upercase", "pr.GET.Upercase")]
        [InlineData("pr", "GET", "https://localhost/api/:colons", "pr.GET.api.__colons")]
        [InlineData("pr", "POST", "https://localhost/api/:colons", "pr.POST.api.__colons")]
        [InlineData("pr", "DELETE", "https://localhost/api/:colons", "pr.DELETE.api.__colons")]
        public async Task UriToPolicyPath(string policyRoot, string method, string uri, string expected)
        {
            var t = new TestAuthorizerAPIClient(policyRoot);
            var builder = TestUtil.GetPolicyWebHostBuilder(t,  options =>
            {
                options.PolicyRoot = policyRoot;
            }, 
            authoritzer =>
            {

            },
            new UriBuilder(uri).Path);
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
        [InlineData("pr", "GET", "api/{asset}", "https://localhost/api/:something", "pr.GET.api.__asset")]
        [InlineData("pr", "GET", "api/{asset}", "https://localhost/api/:upperCase", "pr.GET.api.__asset")]
        [InlineData("pr", "POST", "api/{asset}", "https://localhost/api/:something", "pr.POST.api.__asset")]
        [InlineData("pr", "PUT", "api/{asset}", "https://localhost/api/:something", "pr.PUT.api.__asset")]
        [InlineData("pr", "DELETE", "api/{asset}", "https://localhost/api/:something", "pr.DELETE.api.__asset")]
        [InlineData("pr", "GET", "api/{asset1}/{asset2}", "https://localhost/api/:multiple/:value", "pr.GET.api.__asset1.__asset2")]
        [InlineData("pr", "GET", "api/{asset}/not_last", "https://localhost/api/:value/not_last", "pr.GET.api.__asset.not_last")]
        [InlineData("pr", "GET", "api/{asset}/not_last", "https://localhost/api/val/not_last", "pr.GET.api.__asset.not_last")]
        [InlineData("pr", "GET", "api/{asset}", "https://localhost/api/no_colons", "pr.GET.api.__asset")]
        [InlineData("pr", "GET", "api/{asset}/another/{asset2}", "https://localhost/api/api/another/api", "pr.GET.api.__asset.another.__asset2")]
        [InlineData("pr", "GET", "api/{asset}", "https://localhost/api/UperCase", "pr.GET.api.__asset")]
        [InlineData("pr", "GET", "api/{CaseSensitive}", "https://localhost/api/parameter", "pr.GET.api.__CaseSensitive")]
        // Tests reserver routes. eg. Controller and Action.
        [InlineData("pr", "GET", "api/{controller=Home}/{action=Index}", "https://localhost/api/Home/Account", "pr.GET.api.Home.Account")]
        // Tests optional route parameters
        [InlineData("pr", "GET", "api/{id?}", "https://localhost/api", "pr.GET.api")]
        [InlineData("pr", "GET", "api/{id?}", "https://localhost/api/myid", "pr.GET.api.__id")]
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
