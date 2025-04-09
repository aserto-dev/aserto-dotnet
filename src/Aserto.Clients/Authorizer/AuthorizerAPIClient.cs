//-----------------------------------------------------------------------
// <copyright file="AuthorizerAPIClient.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.Clients.Authorizer
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Aserto.Authorizer.V2;
    using Aserto.Authorizer.V2.Api;
    using Aserto.Clients.Options;
    using Aserto.Clients.Interceptors;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using Grpc.Net.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using static Aserto.Authorizer.V2.Authorizer;
    using System.Linq;
    using System.Net;

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
            logger = loggerFactory.CreateLogger<AuthorizerAPIClient>();

            this.options = options.Value;

            AsertoAuthorizerOptions.Validate(this.options);

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
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
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
                logger.LogDebug("No Authentication type provided. Using Anonymous identity context.");
            }
            else
            {
                logger.LogDebug($"Authentication type set to {isRequest.IdentityContext.Type}. Using identity ${isRequest.IdentityContext.Identity}");
            }

            if (isRequest.PolicyInstance!=null && string.IsNullOrEmpty(isRequest.PolicyInstance.InstanceLabel)){
                isRequest.PolicyInstance.InstanceLabel = isRequest.PolicyInstance.Name;
            }

            logger.LogDebug($"Policy Context path resolved to: {isRequest.PolicyContext.Path}");
            // logger.LogDebug($"Resource Context resolved to: {isRequest.ResourceContext}");
            var result = await authorizerClient.IsAsync(isRequest);

            if (result.Decisions.Count == 0)
            {
                logger.LogDebug("No decisions were returned by the authorizer.");
                return false;
            }

            return result.Decisions[0].Is;
        }

        /// <inheritdoc/>
        public async Task<List<Module>> ListPoliciesAsync(ListPoliciesRequest listRequest)
        {
            if (listRequest == null)
            {
                throw new ArgumentNullException(nameof(listRequest));
            }
            if (listRequest.PolicyInstance!=null && string.IsNullOrEmpty(listRequest.PolicyInstance.InstanceLabel))
            {
                listRequest.PolicyInstance.InstanceLabel = listRequest.PolicyInstance.Name;
            }

            logger.LogDebug($"List Policies - policies instance name: {listRequest.PolicyInstance.Name}");
            logger.LogDebug($"List Policies - policies instance label: {listRequest.PolicyInstance.InstanceLabel}");            
            var result = await authorizerClient.ListPoliciesAsync(listRequest);
            return result.Result.ToList();            
        }

        /// <inheritdoc/>
        public async Task<Module> GetPolicyAsync(GetPolicyRequest getRequest)
        {
            if (getRequest == null)
            {
                throw new ArgumentNullException(nameof(getRequest));
            }
            if (getRequest.PolicyInstance!=null && string.IsNullOrEmpty(getRequest.PolicyInstance.InstanceLabel))
            {
                getRequest.PolicyInstance.InstanceLabel = getRequest.PolicyInstance.Name;
            }
            logger.LogDebug($"Get Policy - policy ID: {getRequest.Id}");
            logger.LogDebug($"Get Policy - policy instance name: {getRequest.PolicyInstance.Name}");
            logger.LogDebug($"Get Policy - policy instance label: {getRequest.PolicyInstance.InstanceLabel}");
            var result = await authorizerClient.GetPolicyAsync(getRequest);
            return result.Result;
        }


        /// <inheritdoc/>
        public async Task<QueryResponse> QueryAsync(QueryRequest queryRequest)
        {
            if (queryRequest == null)
            {
                throw new ArgumentNullException(nameof(queryRequest));
            }
            if (queryRequest.PolicyInstance!=null && string.IsNullOrEmpty(queryRequest.PolicyInstance.InstanceLabel))
            {
                queryRequest.PolicyInstance.InstanceLabel = queryRequest.PolicyInstance.Name;
            }
            logger.LogDebug($"Query Policy: {queryRequest.Query}");
            logger.LogDebug($"Identity Context resolved to: {queryRequest.IdentityContext}");
            var result = await authorizerClient.QueryAsync(queryRequest);
            
            return result;
        }

        /// <inheritdoc/>
        public async Task<CompileResponse> CompileAsync(CompileRequest compileRequest)
        {
            if (compileRequest == null)
            {
                throw new ArgumentNullException(nameof(compileRequest));
            }
            if (compileRequest.PolicyInstance!=null && string.IsNullOrEmpty(compileRequest.PolicyInstance.InstanceLabel))
            {
                compileRequest.PolicyInstance.InstanceLabel = compileRequest.PolicyInstance.Name;
            }

            logger.LogDebug($"Query Policy: {compileRequest.Query}");
            logger.LogDebug($"Identity Context resolved to: {compileRequest.IdentityContext}");
            var result = await authorizerClient.CompileAsync(compileRequest);

            return result;
        }

        /// <inheritdoc/>
        public async Task<DecisionTreeResponse> DecisionTreeAsync(DecisionTreeRequest decisionTreeRequest)
        {
            if (decisionTreeRequest == null)
            {
                throw new ArgumentNullException(nameof(decisionTreeRequest));
            }
            if (decisionTreeRequest.PolicyInstance!=null && string.IsNullOrEmpty(decisionTreeRequest.PolicyInstance.InstanceLabel))
            {
                decisionTreeRequest.PolicyInstance.InstanceLabel = decisionTreeRequest.PolicyInstance.Name;
            }

            logger.LogDebug($"Identity Context resolved to: {decisionTreeRequest.IdentityContext}");
            logger.LogDebug($"Policy Context resolved to: {decisionTreeRequest.PolicyContext}");
            logger.LogDebug($"Resource Context resolved to: {decisionTreeRequest.ResourceContext}");
            var result = await authorizerClient.DecisionTreeAsync(decisionTreeRequest);

            return result;
        }
    }
}
