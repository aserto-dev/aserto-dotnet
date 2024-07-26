//-----------------------------------------------------------------------
// <copyright file="AsertoAuthorizationHandler.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Policies
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Clients.Authorizer;
    using Aserto.Clients.Options;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Aserto Authorization Handler.
    /// </summary>
    internal class AsertoAuthorizationHandler : AuthorizationHandler<AsertoDecisionRequirement>
    {
        private AsertoOptions options;
        private IHttpContextAccessor contextAccessor;
        private IAuthorizerAPIClient authorizerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="options">The Aserto Options.</param>
        /// <param name="contextAccessor">The http context accessor.</param>
        /// <param name="authorizerClient">The authorizer API Client.</param>
        public AsertoAuthorizationHandler(IOptions<AsertoOptions> options, IHttpContextAccessor contextAccessor, IAuthorizerAPIClient authorizerClient)
        {
            this.options = options.Value;
            this.contextAccessor = contextAccessor;
            this.authorizerClient = authorizerClient;
        }

        /// <inheritdoc/>
        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, AsertoDecisionRequirement requirement)
        {
            var httpContext = this.contextAccessor.HttpContext;
            var ok = await this.authorizerClient.IsAsync(this.authorizerClient.BuildIsRequest(httpContext.Request, context.User, requirement.RequiredClaimTypes, this.options));
            if (ok || !this.options.Enabled)
            {
                context.Succeed(requirement);
            }

            await Task.CompletedTask;
        }
    }
}
