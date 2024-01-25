//-----------------------------------------------------------------------
// <copyright file="AsertoMiddleware.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Aserto.AspNetCore.Middleware.Clients;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Authorizer.V2;
    using Google.Api;
    using Grpc.Core;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Middleware used for Aserto Authorization.
    /// </summary>
    public class AsertoMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly IOptionsMonitor<AsertoOptions> optionsMonitor;
        private AsertoOptions options;
        private IAuthorizerAPIClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next request delegate.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="optionsMonitor">The options for the Aserto Middleware.</param>
        /// <param name="client">The <see cref="IAuthorizerAPIClient"/>.</param>
        public AsertoMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptionsMonitor<AsertoOptions> optionsMonitor, IAuthorizerAPIClient client)
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
        }

        /// <summary>
        /// Process requests.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The task.</returns>
        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                this.logger.LogInformation($"Endpoint information for: {context.Request.Path} is null - allowing request");
                await this.next.Invoke(context);
                return;
            }

            var asertoAttribute = endpoint.Metadata.GetMetadata<Extensions.AsertoAttribute>();
            if (asertoAttribute == null)
            {
                this.logger.LogInformation($"Endpoint information for: {context.Request.Path} does not have aserto attribute - allowing request");
                await this.next.Invoke(context);
                return;
            }

            var allowed = await this.client.IsAsync(this.client.BuildIsRequest(context, Utils.DefaultClaimTypes, this.options));
            if (!allowed && this.options.Enabled)
            {
                this.logger.LogInformation($"Decision to allow: {context.Request.Path} was: {allowed}");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                var errorMessage = Encoding.UTF8.GetBytes(HttpStatusCode.Forbidden.ToString());
                await context.Response.Body.WriteAsync(errorMessage, 0, errorMessage.Length);
            }
            else
            {
                this.logger.LogInformation($"Decision to allow: {context.Request.Path} was: {allowed}");
                await this.next.Invoke(context);
            }
        }
    }
}
