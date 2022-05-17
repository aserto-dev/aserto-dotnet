//-----------------------------------------------------------------------
// <copyright file="AuthorizerAPIClient.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;
    using Aserto.API;
    using Aserto.API.V1;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Authorizer.Authorizer.V1;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using static Aserto.Authorizer.Authorizer.V1.Authorizer;

    /// <summary>
    /// Client for Aserto Authorizer API.
    /// </summary>
    public class AuthorizerAPIClient : IAuthorizerAPIClient
    {
        private readonly AuthorizerClient authorizerClient;
        private readonly Grpc.Core.Metadata metaData;
        private readonly AsertoOptions options;
        private readonly string policyRoot;
        private ILogger logger;
        private string decision;
        private string policyID;
        private Func<string, HttpRequest, string> policyPathMapper;
        private Func<string, HttpRequest, Struct> resourceMapper;

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
                var channel = GrpcChannel.ForAddress(
                    this.options.ServiceUrl,
                    new GrpcChannelOptions { });

                this.authorizerClient = new AuthorizerClient(channel);
            }

            this.metaData = new Grpc.Core.Metadata();
            this.metaData.Add("Aserto-Tenant-Id", $"{this.options.TenantID}");
            this.metaData.Add("Authorization", $"basic {this.options.AuthorizerApiKey}");

            this.decision = this.options.Decision;
            this.policyID = this.options.PolicyID;
            this.policyRoot = this.options.PolicyRoot;
            this.policyPathMapper = this.options.PolicyPathMapper;
            this.resourceMapper = this.options.ResourceMapper;
        }

        /// <inheritdoc/>
        public string Decision
        {
            get { return this.decision; }
        }

        /// <inheritdoc/>
        public string PolicyID
        {
            get { return this.policyID; }
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

            var result = await this.authorizerClient.IsAsync(isRequest, this.metaData);

            if (result.Decisions.Count == 0)
            {
                this.logger.LogDebug("No decisions were returned by the authorizer.");
                return false;
            }

            return result.Decisions[0].Is;
        }
    }
}
