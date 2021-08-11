using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Authorizer.Authorizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Xunit;
using static Aserto.Authorizer.Authorizer.Authorizer;

namespace Aserto.AspNetCore.Middleware.Tests.Clients
{
    public class AuthorizerApiClientExtensionsTests
    {
        Moq.Mock<AuthorizerClient> mockAuthorizerClient;
        Moq.Mock<HttpRequest> mockRequest;

        IOptions<AsertoOptions> options;
        ILoggerFactory loggerFactory;

        public AuthorizerApiClientExtensionsTests()
        {
            this.mockAuthorizerClient = new Moq.Mock<AuthorizerClient>();
            this.options = Microsoft.Extensions.Options.Options.Create(new AsertoOptions());
            this.loggerFactory = new NullLoggerFactory();

            this.mockRequest = new Moq.Mock<HttpRequest>();
        }

        [Fact]
        public void RequestNullThrows()
        {
            HttpRequest request = null;
            ClaimsPrincipal claimsPrinipal = new DefaultHttpContext().User;

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, loggerFactory, mockAuthorizerClient.Object);

            Assert.Throws<ArgumentNullException>(() => authorizerAPIClient.BuildIsRequest(request, claimsPrinipal, Utils.DefaultClaimTypes));
        }

        [Fact]
        public void ClaimsNullThrows()
        {
            HttpRequest request = new DefaultHttpContext().Request;
            ClaimsPrincipal claimsPrinipal = null;

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);

            Assert.Throws<ArgumentNullException>(() => authorizerAPIClient.BuildIsRequest(request, claimsPrinipal, Utils.DefaultClaimTypes));
        }

        [Fact]
        public void NullAuthenticationTypeUsesAnonymous()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var testIdentity = new ClaimsIdentity();
            var testPrincipal = new ClaimsPrincipal(testIdentity);


            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(this.mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);

            Assert.Equal(API.IdentityType.None, isRequest.IdentityContext.Type);
        }

        [Fact]
        public void EmailClaimsExistsUsesEmailIdentity()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim(ClaimTypes.Name, "userName"),
                new Claim(ClaimTypes.Email, "email"),
            };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);

            Assert.Equal(API.IdentityType.Sub, isRequest.IdentityContext.Type);
            Assert.Equal("email", isRequest.IdentityContext.Identity);
        }

        [Fact]
        public void NoEmailClaimsUsesNameIdentity()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim(ClaimTypes.Name, "userName"),
            };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);

            Assert.Equal(API.IdentityType.Sub, isRequest.IdentityContext.Type);
            Assert.Equal("userName", isRequest.IdentityContext.Identity);
        }

        [Fact]
        public void NoEmailNoUserClaimsUsesNameIdentifierIdentity()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "userName"),
            };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);

            Assert.Equal(API.IdentityType.Sub, isRequest.IdentityContext.Type);
            Assert.Equal("userName", isRequest.IdentityContext.Identity);
        }

        [Fact]
        public void CustomClaimsTypeAllows()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new[]
            {
                new Claim("my_custom_claim_type", "custom_value"),
            };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, new List<string>() { "my_custom_claim_type" });

            Assert.Equal(API.IdentityType.Sub, isRequest.IdentityContext.Type);
            Assert.Equal("custom_value", isRequest.IdentityContext.Identity);
        }


        [Fact]
        public void MultipleCustomClaimsTypeAUsesFirstFound()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new[]
            {
                new Claim("my_custom_claim_type1", "custom_value1"),
                new Claim("my_custom_claim_type2", "custom_value2"),
            };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, new List<string>() { "my_custom_claim_type1", "my_custom_claim_type2" });

            Assert.Equal(API.IdentityType.Sub, isRequest.IdentityContext.Type);
            Assert.Equal("custom_value1", isRequest.IdentityContext.Identity);
        }

        [Fact]
        public void NoKnownClaimsUsesAnonymous()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new[]
            {
                new Claim(ClaimTypes.Country, "mycountry"),
            };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);

            Assert.Equal(API.IdentityType.None, isRequest.IdentityContext.Type);
        }

        [Fact]
        public void NoClaimsUsesAnonymous()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new Claim[] { };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);

            Assert.Equal(API.IdentityType.None, isRequest.IdentityContext.Type);
        }

        [Theory]
        [InlineData("tp", "GET", "/", "tp.GET")]
        [InlineData("tp", "GET", "", "tp.GET")]
        [InlineData("tp", "POST", "", "tp.POST")]
        [InlineData("tp", "PUT", "", "tp.PUT")]
        [InlineData("tp", "DELETE", "", "tp.DELETE")]
        [InlineData("t", "GET", "/", "t.GET")]
        [InlineData("tp", "GET", "/foo", "tp.GET.foo")]
        [InlineData("tp", "GET", "/foo/", "tp.GET.foo")]
        [InlineData("tp", "GET", "/foo/bar", "tp.GET.foo.bar")]
        [InlineData("tp", "GET", "/foo/bar/", "tp.GET.foo.bar")]
        [InlineData("tp", "POST", "/foo/bar/", "tp.POST.foo.bar")]
        [InlineData("tp", "DELETE", "/foo/bar/", "tp.DELETE.foo.bar")]
        public void PolicyPathBuilds(string policyRoot, string method, string path, string expected)
        {
            this.mockRequest.SetupGet(r => r.Path).Returns(path);
            this.mockRequest.SetupGet(r => r.Method).Returns(method);
            var testIdentity = new ClaimsIdentity();
            var testPrincipal = new ClaimsPrincipal(testIdentity);
            this.options.Value.PolicyRoot = policyRoot;

            var authorizerAPIClient = new AuthorizerAPIClient(this.options, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);

            Assert.Equal(expected, isRequest.PolicyContext.Path);
        }
    }
}
