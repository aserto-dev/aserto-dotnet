using Aserto.AspNetCore.Middleware.Clients;
using Aserto.AspNetCore.Middleware.Options;
using Aserto.Authorizer.Authorizer;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Aserto.Authorizer.Authorizer.Authorizer;

namespace Aserto.AspNetCore.Middleware.Tests.Clients
{
    public class AuthorizerApiClientTests
    {
        [Fact]
        public void RequestNullThrows()
        {
            var mockClient = new Moq.Mock<AuthorizerClient>();
            var options = Microsoft.Extensions.Options.Options.Create(new AsertoOptions());
            var logggerFactory = new NullLoggerFactory();
            IsRequest isRequest = null;

            Assert.ThrowsAsync<ArgumentNullException>(() => new AuthorizerAPIClient(options, logggerFactory, mockClient.Object).IsAsync(isRequest));
        }

        [Fact]
        public async Task AuthenticationTypeUsesAnonymous()
        {
            var mockClient = new Moq.Mock<AuthorizerClient>();
            var mockClaimsPrinipal = new Moq.Mock<ClaimsPrincipal>();
            var mockRequest = new Moq.Mock<HttpRequest>();

            var options = Microsoft.Extensions.Options.Options.Create(new AsertoOptions());
            var logggerFactory = new NullLoggerFactory();

            var isResponse = new IsResponse();
            isResponse.Decisions.Add(new Decision() { Is = true });

            mockClaimsPrinipal.SetupGet(cp => cp.Identity.AuthenticationType).Returns(() => null) ;
            mockRequest.SetupGet(r => r.Path).Returns("/foo");
            mockRequest.SetupGet(r => r.Method).Returns("GET");

            var fakecall = TestCalls.AsyncUnaryCall<IsResponse>(Task.FromResult(isResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(), () => { });


            mockClient.Setup(c => c.IsAsync(Moq.It.IsAny<IsRequest>(), Moq.It.IsAny<Metadata>(), null, CancellationToken.None)).Returns(fakecall);

            var authorizerAPIClient = new AuthorizerAPIClient(options, logggerFactory, mockClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, mockClaimsPrinipal.Object, Utils.DefaultClaimTypes);
            var isAsync = await authorizerAPIClient.IsAsync(isRequest);
            
            Assert.True(isAsync);
        }

        [Fact]
        public async Task AuthenticationTypeUsesManual()
        {
            var mockClient = new Moq.Mock<AuthorizerClient>();
            var mockRequest = new Moq.Mock<HttpRequest>();

            var options = Microsoft.Extensions.Options.Options.Create(new AsertoOptions());
            var logggerFactory = new NullLoggerFactory();

            var isResponse = new IsResponse();
            isResponse.Decisions.Add(new Decision() { Is = true });

            mockRequest.SetupGet(r => r.Path).Returns("/foo");
            mockRequest.SetupGet(r => r.Method).Returns("GET");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim(ClaimTypes.Name, "userName"),
            };
            var testIdentity = new ClaimsIdentity(claims, "test authentication");
            var testPrincipal = new ClaimsPrincipal(testIdentity);

            var fakecall = TestCalls.AsyncUnaryCall<IsResponse>(Task.FromResult(isResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(), () => { });


            mockClient.Setup(c => c.IsAsync(Moq.It.IsAny<IsRequest>(), Moq.It.IsAny<Metadata>(), null, CancellationToken.None)).Returns(fakecall);

            var authorizerAPIClient = new AuthorizerAPIClient(options, logggerFactory, mockClient.Object);
            var isRequest = authorizerAPIClient.BuildIsRequest(mockRequest.Object, testPrincipal, Utils.DefaultClaimTypes);
            var isAsync = await authorizerAPIClient.IsAsync(isRequest);

            Assert.True(isAsync);
        }
    }
}
