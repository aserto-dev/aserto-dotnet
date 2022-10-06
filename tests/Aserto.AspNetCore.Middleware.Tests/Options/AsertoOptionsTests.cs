using Aserto.AspNetCore.Middleware.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Aserto.AspNetCore.Middleware.Tests.Options
{
    public class AsertoOptionsTests
    {
        private AsertoOptions getValidOptions()
        {
            var options = new AsertoOptions();
            options.AuthorizerApiKey = "YOUR_AUTHORIZER_API_KEY";
            options.TenantID = "YOUR_TENANT_ID";
            options.PolicyName = "YOUR_POLICY_NAME";
            options.PolicyRoot = "weatherforecast";
            return options;
        }

        [Theory]
        [InlineData("invalidUrl")]
        [InlineData("ftp://testserver.com")]
        public void InvalidUrlDenies(string serviceUrl)
        {
            var options = getValidOptions();
            options.ServiceUrl = serviceUrl;

            Assert.False(AsertoOptions.Validate(options));
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

            Assert.True(AsertoOptions.Validate(options));
        }

        [Fact]
        public void NoAuthorizerApiKeyAllows()
        {
            var options = getValidOptions();
            options.AuthorizerApiKey = string.Empty;

            Assert.True(AsertoOptions.Validate(options));
        }

        [Fact]
        public void NoPolicyNameAllows()
        {
            var options = getValidOptions();
            options.PolicyName = string.Empty;

            Assert.True(AsertoOptions.Validate(options));
        }

        [Fact]
        public void NoPolicyRootDenies()
        {
            var options = getValidOptions();
            options.PolicyRoot = string.Empty;

            Assert.False(AsertoOptions.Validate(options));
        }

        [Fact]
        public void NoTenantIDAllows()
        {
            var options = getValidOptions();
            options.TenantID = string.Empty;

            Assert.True(AsertoOptions.Validate(options));
        }

        [Fact]
        public void NullOptionsThrow()
        {
            AsertoOptions options = null;

            Assert.Throws<ArgumentNullException>(() => AsertoOptions.Validate(options));
        }
    }
}
