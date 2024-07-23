//-----------------------------------------------------------------------
// <copyright file="AuthorizerAPIClientExtensions.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Authorizer.V2;
    using Aserto.Authorizer.V2.Api;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Additional operations that can be performed against a <see cref="IAuthorizerAPIClient"/>.
    /// </summary>
    public static class AuthorizerAPIClientExtensions
    {
        /// <summary>
        /// Creates an <see cref="IsRequest"/> use in the Is call.
        /// </summary>
        /// <param name="client">The <see cref="IAuthorizerAPIClient"/>.</param>
        /// <param name="context">The <see cref="HttpContext"/> to use for building the Is request.</param>
        /// <param name="supportedClaimTypes">A list of Claim types to check.</param>
        /// <param name="options"><see cref="AsertoOptions"/>.A list of options to build the request.</param>
        /// <returns>A new <see cref="IsRequest"/> configured for the <paramref name="context"/>.</returns>
        public static IsRequest BuildIsRequest(this IAuthorizerAPIClient client, HttpContext context, IEnumerable<string> supportedClaimTypes, AsertoOptions options)
        {
            return BuildIsRequest(client, context.Request, context.User, supportedClaimTypes, options);
        }

        /// <summary>
        /// Creates an <see cref="IsRequest"/> use in the Is call.
        /// </summary>
        /// <param name="client">The <see cref="IAuthorizerAPIClient"/>.</param>
        /// <param name="request">The <see cref="HttpRequest"/> to use for building the Is request.</param>
        /// <param name="identity">The <see cref="ClaimsPrincipal"/> to use for building the Is request.</param>
        /// <param name="supportedClaimTypes">A list of Claim types to check.</param>
        /// <param name="options">The <see cref="AsertoOptions"/> A list of options to build the request.</param>
        /// <returns>A new <see cref="IsRequest"/> configured for the <paramref name="request"/> and the <paramref name="identity"/>.</returns>
        public static IsRequest BuildIsRequest(this IAuthorizerAPIClient client, HttpRequest request, ClaimsPrincipal identity, IEnumerable<string> supportedClaimTypes, AsertoOptions options)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            var isRequest = new IsRequest();
            var policyContext = new PolicyContext();

            var identityContext = new IdentityContext();
            if (options.IdentityMapper == null)
            {
                identityContext = BuildIdentityContext(identity, supportedClaimTypes);
            }
            else
            {
                identityContext = options.IdentityMapper(identity, supportedClaimTypes);
            }

            var policyPath = options.PolicyPathMapper(options.PolicyRoot, request);
            policyContext.Path = policyPath;

            policyContext.Decisions.Add(options.Decision);

            if (!string.IsNullOrEmpty(options.PolicyName) || !string.IsNullOrEmpty(options.PolicyInstanceLabel))
            {
                var policyInstance = new PolicyInstance
                {
                    InstanceLabel = options.PolicyInstanceLabel,
                    Name = options.PolicyInstanceLabel,
                };
                if (string.IsNullOrEmpty(options.PolicyInstanceLabel))
                {
                    policyInstance.InstanceLabel = policyInstance.Name;
                }

                isRequest.PolicyInstance = policyInstance;
            }

            isRequest.IdentityContext = identityContext;
            isRequest.PolicyContext = policyContext;

            isRequest.ResourceContext = options.ResourceMapper(options.PolicyRoot, request);

            return isRequest;
        }

        private static IdentityContext BuildIdentityContext(ClaimsPrincipal identity, IEnumerable<string> supportedClaimTypes)
        {
            var identityContext = new IdentityContext();
            identityContext.Type = IdentityType.None;

            if (identity.Identity.AuthenticationType != null && identity.Identity != null)
            {
                foreach (string supportedClaimType in supportedClaimTypes)
                {
                    var claim = identity.FindFirst(c => c.Type == supportedClaimType);
                    if (claim != null)
                    {
                        identityContext.Type = IdentityType.Sub;
                        identityContext.Identity = claim.Value;
                        break;
                    }
                }
            }

            return identityContext;
        }
    }
}
