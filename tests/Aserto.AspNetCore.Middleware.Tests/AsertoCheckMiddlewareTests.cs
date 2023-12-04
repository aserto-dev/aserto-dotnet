using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Extensions;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.AspNetCore.Middleware.Tests.Testing;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    public class AsertoCheckMiddlewareTests
    {
        [Fact]
        public async Task AsertoCheckAllows()
        {

            var checkOptions = new CheckOptions();
            checkOptions.BaseOptions = new AsertoOptions();
            checkOptions.BaseOptions.PolicyName = "test";
            checkOptions.BaseOptions.PolicyInstanceLabel = "test";
            checkOptions.BaseOptions.PolicyRoot = "testRoot";
            checkOptions.BaseOptions.ServiceUrl = "https://testserver.com";
            checkOptions.BaseOptions.Enabled = false;

            var checkResourceRules = new Dictionary<string, Func<string, HttpRequest, Struct>>();

            checkResourceRules.Add("admin", (policyRoot, httpRequest) =>
            {
                Struct result = new Struct();
                result.Fields.Add("object_id", Value.ForString("admin"));
                result.Fields.Add("object_type", Value.ForString("group"));
                result.Fields.Add("relation", Value.ForString("member"));
                return result;
            });

            checkOptions.ResourceMappingRules = checkResourceRules;

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAsertoCheckAuthorization(checkOptions);
                    TestAuthorizerAPIClient t = new TestAuthorizerAPIClient();
                    services.AddSingleton<IAuthorizerAPIClient, TestAuthorizerAPIClient>(t =>
                    {
                        return new TestAuthorizerAPIClient(true);
                    });
                })
                .Configure(app =>
                {
                    app.UseAsertoCheckAuthorization();
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetAsync("/");
        }
    }
}
