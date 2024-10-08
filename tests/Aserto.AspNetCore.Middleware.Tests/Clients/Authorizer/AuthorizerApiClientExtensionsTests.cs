﻿using Aserto.AspNetCore.Middleware.Options;
using Aserto.Authorizer.V2.Api;
using Aserto.Clients.Authorizer;
using Aserto.Clients.Options;
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
using static Aserto.Authorizer.V2.Authorizer;

namespace Aserto.AspNetCore.Middleware.Tests.Clients
{
    public class AuthorizerApiClientExtensionsTests
    {
        Moq.Mock<AuthorizerClient> mockAuthorizerClient;
        Moq.Mock<HttpRequest> mockRequest;

        AsertoOptions asertoOptions;
        IOptions<AsertoAuthorizerOptions> authorizerOptions;
        ILoggerFactory loggerFactory;

        public AuthorizerApiClientExtensionsTests()
        {
            this.mockAuthorizerClient = new Moq.Mock<AuthorizerClient>();
            this.asertoOptions = new AsertoOptions();            
            this.authorizerOptions = Microsoft.Extensions.Options.Options.Create(new AsertoAuthorizerOptions());
            this.loggerFactory = new NullLoggerFactory();

            this.mockRequest = new Moq.Mock<HttpRequest>();
        }

        [Fact]
        public void RequestNullThrows()
        {
            HttpRequest request = null;
            ClaimsPrincipal claimsPrinipal = new DefaultHttpContext().User;

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, loggerFactory, mockAuthorizerClient.Object);

            Assert.Throws<ArgumentNullException>(() => authorizerAPIClient.BuildIsRequest(request, claimsPrinipal, Utils.DefaultClaimTypes, this.asertoOptions));
        }

        [Fact]
        public void ClaimsNullThrows()
        {
            HttpRequest request = new DefaultHttpContext().Request;
            ClaimsPrincipal claimsPrinipal = null;

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);

            Assert.Throws<ArgumentNullException>(() => authorizerAPIClient.BuildIsRequest(request, claimsPrinipal, Utils.DefaultClaimTypes, this.asertoOptions));
        }

        [Fact]
        public void NullAuthenticationTypeUsesAnonymous()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var testIdentity = new ClaimsIdentity();
            var testPrincipal = new ClaimsPrincipal(testIdentity);


            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(this.mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal(IdentityType.None, isRequest.IdentityContext.Type);
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

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal(IdentityType.Sub, isRequest.IdentityContext.Type);
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

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal(IdentityType.Sub, isRequest.IdentityContext.Type);
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

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal(IdentityType.Sub, isRequest.IdentityContext.Type);
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

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, new List<string>() { "my_custom_claim_type" }, this.asertoOptions);

            Assert.Equal(IdentityType.Sub, isRequest.IdentityContext.Type);
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

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, new List<string>() { "my_custom_claim_type1", "my_custom_claim_type2" }, this.asertoOptions);

            Assert.Equal(IdentityType.Sub, isRequest.IdentityContext.Type);
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

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal(IdentityType.None, isRequest.IdentityContext.Type);
        }

        [Fact]
        public void NoClaimsUsesAnonymous()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var claims = new Claim[] { };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal(IdentityType.None, isRequest.IdentityContext.Type);
        }

        [Theory]
        [InlineData("polroot","GET", "/", "polroot.GET")]
        [InlineData("polroot", "GET", "", "polroot.GET")]
        [InlineData("polroot", "POST", "", "polroot.POST")]
        [InlineData("polroot", "PUT", "", "polroot.PUT")]
        [InlineData("polroot", "DELETE", "", "polroot.DELETE")]
        [InlineData("polroot", "GET", "/foo", "polroot.GET.foo")]
        [InlineData("polroot", "GET", "/foo/", "polroot.GET.foo")]
        [InlineData("polroot", "GET", "/foo/bar", "polroot.GET.foo.bar")]
        [InlineData("polroot", "GET", "/foo/bar/", "polroot.GET.foo.bar")]
        [InlineData("polroot", "POST", "/foo/bar/", "polroot.POST.foo.bar")]
        [InlineData("polroot", "DELETE", "/foo/bar/", "polroot.DELETE.foo.bar")]
        public void PolicyPathBuilds(string policyRoot,string method, string path, string expected)
        {
            this.mockRequest.SetupGet(r => r.Path).Returns(path);
            this.mockRequest.SetupGet(r => r.Method).Returns(method);
            var testIdentity = new ClaimsIdentity();
            var testPrincipal = new ClaimsPrincipal(testIdentity);
            this.asertoOptions.PolicyRoot = policyRoot;

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal(expected, isRequest.PolicyContext.Path);
        }

        [Fact]
        public void CustomURLToPolicyMapper()
        {
            this.mockRequest.SetupGet(r => r.Path).Returns("/foo");
            this.mockRequest.SetupGet(r => r.Method).Returns("GET");
            var testIdentity = new ClaimsIdentity();
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            this.asertoOptions.PolicyPathMapper = (policyRoot, httpRequest) =>
            {
                return "custom.policy.mapper";
            };

            var authorizerAPIClient = new AuthorizerAPIClient(this.authorizerOptions, this.loggerFactory, this.mockAuthorizerClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes, this.asertoOptions);

            Assert.Equal("custom.policy.mapper", isRequest.PolicyContext.Path);
        }
    }
}
