using Aserto.AspNetCore.Middleware.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Aserto.AspNetCore.Middleware.Tests.Options
{
    public class AsertoAuthorizerOptionsTest
    {
        private AsertoAuthorizerOptions getValidOptions()
        {
            var options = new AsertoAuthorizerOptions();
            options.AuthorizerApiKey = "YOUR_AUTHORIZER_API_KEY";
            options.TenantID = "YOUR_TENANT_ID";
            return options;
        }

        [Theory]
        [InlineData("invalidUrl")]
        [InlineData("ftp://testserver.com")]
        public void InvalidUrlDenies(string serviceUrl)
        {
            var options = getValidOptions();
            options.ServiceUrl = serviceUrl;

            Assert.False(AsertoAuthorizerOptions.Validate(options));
        }

        [Theory]
        [InlineData("https://testserver.com")]
        [InlineData("http://testserver.com")]
        [InlineData("http://testserver.com:8080")]
        [InlineData("http://testserver.com:8080/")]
        [InlineData("http://testserver.com:8080/myservice")]
        public void ValidUrlAllows(string serviceUrl)
        {
            var options = getValidOptions();
            options.ServiceUrl = serviceUrl;

            Assert.True(AsertoAuthorizerOptions.Validate(options));
        }

        [Fact]
        public void NoAuthorizerApiKeyAllows()
        {
            var options = getValidOptions();
            options.AuthorizerApiKey = string.Empty;

            Assert.True(AsertoAuthorizerOptions.Validate(options));
        }

        [Fact]
        public void NoTenantIDAllows()
        {
            var options = getValidOptions();
            options.TenantID = string.Empty;

            Assert.True(AsertoAuthorizerOptions.Validate(options));
        }
    }
}
