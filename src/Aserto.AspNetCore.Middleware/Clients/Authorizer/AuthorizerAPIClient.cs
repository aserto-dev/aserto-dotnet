//-----------------------------------------------------------------------
// <copyright file="AuthorizerAPIClient.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Authorizer.V2;
    using Aserto.Authorizer.V2.API;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using static Aserto.Authorizer.V2.Authorizer;

    /// <summary>
    /// Client for Aserto Authorizer API.
    /// </summary>
    public class AuthorizerAPIClient : IAuthorizerAPIClient
    {
        private readonly AuthorizerClient authorizerClient;
        private readonly AsertoOptions options;
        private readonly string decision;
        private readonly ILogger logger;
        private readonly string policyName;
        private readonly string policyInstanceLabel;
        private readonly string policyRoot;
        private readonly Func<string, HttpRequest, string> policyPathMapper;
        private readonly Func<string, HttpRequest, Struct> resourceMapper;
        private readonly Func<ClaimsPrincipal, IEnumerable<string>, IdentityContext> identityMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizerAPIClient"/> class.
        /// </summary>
        /// <param name="options">Authorizer API Client options <see cref="AsertoOptions"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for this class.</param>
        /// <param name="authorizerClient">Optional <see cref="AuthorizerClient"/> to use when sending requests.</param>
        public AuthorizerAPIClient(IOptions<AsertoOptions> options, ILoggerFactory loggerFactory, AuthorizerClient authorizerClient = null)
        {
            this.logger = loggerFactory.CreateLogger<AuthorizerAPIClient>();

            this.options = options.Value;
            if (authorizerClient != null)
            {
                this.authorizerClient = authorizerClient;
            }
            else
            {
                this.authorizerClient = null;

                var grpcChannelOptions = new GrpcChannelOptions { };

                if (this.options.Insecure)
                {
                    var httpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    };

                    grpcChannelOptions = new GrpcChannelOptions { HttpHandler = httpHandler };
                }

                var interceptor = new HeaderInterceptor(this.options.AuthorizerApiKey, this.options.TenantID);

                var channel = GrpcChannel.ForAddress(
                    this.options.ServiceUrl,
                    grpcChannelOptions);

                var invoker = channel.Intercept(interceptor);

                this.authorizerClient = new AuthorizerClient(invoker);
            }

            this.decision = this.options.Decision;
            this.policyName = this.options.PolicyName;
            this.policyInstanceLabel = this.options.PolicyInstanceLabel;
            this.policyPathMapper = this.options.PolicyPathMapper;
            this.resourceMapper = this.options.ResourceMapper;
            this.identityMapper = this.options.IdentityMapper;
            this.policyRoot = this.options.PolicyRoot;
        }

        /// <inheritdoc/>
        public string Decision
        {
            get { return this.decision; }
        }

        /// <inheritdoc/>
        public string PolicyName
        {
            get { return this.policyName; }
        }

        /// <inheritdoc/>
        public string PolicyInstanceLabel
        {
            get { return this.policyInstanceLabel; }
        }

        /// <inheritdoc/>
        public string PolicyRoot
        {
            get { return this.policyRoot; }
        }

        /// <inheritdoc/>
        public Func<string, HttpRequest, string> PolicyPathMapper
        {
            get { return this.policyPathMapper; }
        }

        /// <inheritdoc/>
        public Func<string, HttpRequest, Struct> ResourceMapper
        {
            get { return this.resourceMapper; }
        }

        /// <inheritdoc/>
        public Func<ClaimsPrincipal, IEnumerable<string>, IdentityContext> IdentityMapper
        {
            get { return this.identityMapper; }
        }

        /// <inheritdoc/>
        public async Task<bool> IsAsync(IsRequest isRequest)
        {
            if (isRequest == null)
            {
                throw new ArgumentNullException(nameof(isRequest));
            }

            if (isRequest.IdentityContext.Type == IdentityType.None)
            {
                this.logger.LogDebug("No Authentication type provided. Using Anonymous identity context.");
            }
            else
            {
                this.logger.LogDebug($"Authentication type set to {isRequest.IdentityContext.Type}. Using identity ${isRequest.IdentityContext.Identity}");
            }

            this.logger.LogDebug($"Policy Context path resolved to: {isRequest.PolicyContext.Path}");
            this.logger.LogDebug($"Resource Context resolved to: {isRequest.ResourceContext}");
            var result = await this.authorizerClient.IsAsync(isRequest);

            if (result.Decisions.Count == 0)
            {
                this.logger.LogDebug("No decisions were returned by the authorizer.");
                return false;
            }

            return result.Decisions[0].Is;
        }
    }
}
