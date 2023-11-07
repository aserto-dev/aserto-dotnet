//-----------------------------------------------------------------------
// <copyright file="CheckMiddleware.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Aserto.AspNetCore.Middleware.Clients;
    using Aserto.AspNetCore.Middleware.Extensions;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Authorizer.V2;
    using Aserto.Directory.Reader.V3;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Middleware used for Aserto Check Authorization.
    /// </summary>
    public class CheckMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly IOptionsMonitor<CheckOptions> optionsMonitor;
        private CheckOptions options;
        private IAuthorizerAPIClient client;
        private Dictionary<string, Func<string, HttpRequest, Struct>> resourceMappingRules = new Dictionary<string, Func<string, HttpRequest, Struct>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next request delegate.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="optionsMonitor">The options for the Aserto Middleware.</param>
        /// <param name="client">The <see cref="IAuthorizerAPIClient"/>.</param>
        public CheckMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptionsMonitor<CheckOptions> optionsMonitor, IAuthorizerAPIClient client)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.logger = loggerFactory.CreateLogger<AsertoMiddleware>();

            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            this.options = optionsMonitor.CurrentValue;
            this.optionsMonitor.OnChange(options =>
            {
                // Clear the cached settings so the next EnsuredConfigured will re-evaluate.
                this.options = options;
            });
            this.client = client;
            this.resourceMappingRules = this.options.ResourceMappingRules;
        }

        /// <summary>
        /// Process requests.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The task.</returns>
        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var checkAttribute = endpoint.Metadata.GetMetadata<Extensions.CheckAttribute>();

                Func<string, HttpRequest, Struct> resourceMapper = null;
                if (checkAttribute != null)
                {
                    if (this.resourceMappingRules.TryGetValue(checkAttribute.ResourceMapper, out resourceMapper))
                    {
                        this.client.ResourceMapper = resourceMapper;
                    }
                }

                var request = this.client.BuildIsRequest(context, Utils.DefaultClaimTypes);

                var allowed = await this.client.IsAsync(request);
                if (!allowed && this.options.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                else
                {
                    this.logger.LogInformation($"Decision to allow: {context.Request.Path} was: {allowed}");
                    await this.next.Invoke(context);
                }
            }
            else
            {
                this.logger.LogInformation($"Endpoint information for: {context.Request.Path} is null - allowing request");
                await this.next.Invoke(context);
            }
        }
    }
}
