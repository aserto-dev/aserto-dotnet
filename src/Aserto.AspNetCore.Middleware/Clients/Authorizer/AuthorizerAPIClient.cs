﻿//-----------------------------------------------------------------------
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
        private readonly AsertoAuthorizerOptions options;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizerAPIClient"/> class.
        /// </summary>
        /// <param name="options">Authorizer API Client options <see cref="AsertoAuthorizerOptions"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for this class.</param>
        /// <param name="authorizerClient">Optional <see cref="AuthorizerClient"/> to use when sending requests.</param>
        public AuthorizerAPIClient(IOptions<AsertoAuthorizerOptions> options, ILoggerFactory loggerFactory, AuthorizerClient authorizerClient = null)
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
