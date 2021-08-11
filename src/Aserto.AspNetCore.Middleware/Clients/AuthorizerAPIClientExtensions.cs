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
    using System.Text;
    using Aserto.API;
    using Aserto.Authorizer.Authorizer;
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
        /// <returns>A new <see cref="IsRequest"/> configured for the <paramref name="context"/>.</returns>
        public static IsRequest BuildIsRequest(this IAuthorizerAPIClient client, HttpContext context, IEnumerable<string> supportedClaimTypes)
        {
            return BuildIsRequest(client, context.Request, context.User, supportedClaimTypes);
        }

        /// <summary>
        /// Creates an <see cref="IsRequest"/> use in the Is call.
        /// </summary>
        /// <param name="client">The <see cref="IAuthorizerAPIClient"/>.</param>
        /// <param name="request">The <see cref="HttpRequest"/> to use for building the Is request.</param>
        /// <param name="identity">The <see cref="ClaimsPrincipal"/> to use for building the Is request.</param>
        /// <param name="supportedClaimTypes">A list of Claim types to check.</param>
        /// <returns>A new <see cref="IsRequest"/> configured for the <paramref name="request"/> and the <paramref name="identity"/>.</returns>
        public static IsRequest BuildIsRequest(this IAuthorizerAPIClient client, HttpRequest request, ClaimsPrincipal identity, IEnumerable<string> supportedClaimTypes)
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
            var identityContext = new IdentityContext();
            var policyContext = new PolicyContext();
            identityContext.Mode = IdentityMode.Anonymous;
            identityContext.Type = IdentityType.None;

            if (identity.Identity.AuthenticationType != null && identity.Identity != null)
            {
                foreach (string supportedClaimType in supportedClaimTypes)
                {
                    var claim = identity.FindFirst(c => c.Type == supportedClaimType);
                    if (claim != null)
                    {
                        identityContext.Mode = IdentityMode.Manual;
                        identityContext.Type = IdentityType.Sub;
                        identityContext.Identity = claim.Value;
                        break;
                    }
                }
            }

            var policyPath = BuildPolicyPath(client.PolicyRoot, request.Method, request.Path.Value);
            policyContext.Path = policyPath;
            policyContext.Id = client.PolicyID;
            policyContext.Decisions.Add(client.Decision);

            isRequest.IdentityContext = identityContext;
            isRequest.PolicyContext = policyContext;

            // TODO: handle resource context.
            isRequest.ResourceContext = null;

            return isRequest;
        }

        private static string BuildPolicyPath(string policyRoot, string method, string urlPath)
        {
            var policyPath = policyRoot;

            policyPath = $"{policyPath}.{method.ToUpper()}";
            policyPath = $"{policyPath}{urlPath.Replace("/", ".").ToLower()}".TrimEnd('.');

            return policyPath;
        }
    }
}
