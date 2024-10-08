﻿//-----------------------------------------------------------------------
// <copyright file="AsertoOptionsDefaults.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.Middleware.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
    using System.Web;
    using Aserto.Authorizer.V2.Api;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.Owin;

    // Reserved routing names: https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#reserved-routing-names

    /// <summary>
    /// Defaults for Aserto Options.
    /// </summary>
    public static class AsertoOptionsDefaults
    {
        private static readonly string[] ReservedRoutes = { "action", "area", "controller", "handler", "page" };

        /// <summary>
        /// Gets a value indicating whether the Aserto authorization is enabled.
        /// </summary>
        public static bool Enabled { get; } = true;

        /// <summary>
        /// Gets a value indicating the Aserto service URL.
        /// </summary>
        public static string ServiceUrl { get; } = "https://localhost:8282";

        /// <summary>
        /// Gets a value indicating the Aserto service URL.
        /// </summary>
        public static string DirectoryServiceUrl { get; } = "https://localhost:9292";

        /// <summary>
        /// Gets a value indicating the Aserto Authorizer API Key.
        /// </summary>
        public static string AuthorizerApiKey { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the Aserto Tenant Identifier.
        /// </summary>
        public static string TenantID { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the Directory Aserto Tenant Identifier.
        /// </summary>
        public static string DirectoryTenantID { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the Policy Name.
        /// </summary>
        public static string PolicyName { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the service connections are allowed when using SSL.
        /// </summary>
        public static bool Insecure { get; } = false;

        /// <summary>
        /// Gets a value indicating the Policy Name.
        /// </summary>
        public static string PolicyInstanceLabel { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the Aserto Policy Root.
        /// </summary>
        public static string PolicyRoot { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the decision string.
        /// </summary>
        public static string Decision { get; } = "allowed";

        /// <summary>
        /// The default Policy Path Mapper.
        /// </summary>
        /// <param name="policyRoot">The policy root.</param>
        /// <param name="request">The <see cref="HttpRequest"/>.</param>
        /// <returns>The Aserto Policy path.</returns>
        public static string DefaultPolicyPathMapper(string policyRoot, dynamic request)
        {
            string policyPath = "";
            string method = "";
            if (request is IOwinRequest)
            {
                policyPath = request.Path.ToString();
                method = request.Method.ToUpper();
            }
            else
            {
                if (request is System.Web.HttpRequestWrapper)
                {
                  policyPath = request.Path.ToString();
                  method = request.Method.ToUpper();
                }
                else 
                {
                  policyPath = request.RequestUri.LocalPath.ToString();
                  method = request.Method.Method.ToUpper();
                }
            }


            // replace "{" with "__" in endpoint
            policyPath = policyPath.Replace("{", "__");

            // replace "}" with ""
            policyPath = policyPath.Replace("}", string.Empty);

            // Replace "/" with "."
            policyPath = policyPath.Replace("/", ".");

            // Handle any other ":"
            policyPath = policyPath.Replace(":", "__");

            // Handle method
            policyPath = $"{method}.{policyPath.TrimStart('.')}";

            // Trim tailing dots
            policyPath = policyPath.TrimEnd('.');

            Regex regex = new Regex("[^a-zA-Z0-9._]");
            policyPath = regex.Replace(policyPath, "_");

            // Append policyRoot
            policyPath = $"{policyRoot}.{policyPath}";

            return policyPath;
        }

        /// <summary>
        /// The default Resource Context mapper function.
        /// </summary>
        /// <param name="policyRoot">The policy root.</param>
        /// <param name="request">The <see cref="HttpRequest"/>.</param>
        /// <returns>The default Resource Context mapper.</returns>
        public static Struct DefaultResourceMapper(string policyRoot, dynamic request)
        {
            if (request == null)
            {
                return null;
            }

            Struct result = new Struct();

            if (request is OwinRequest)
            {
                foreach (var routeValue in request.Environment)
                {
                    bool exists = Array.Exists(ReservedRoutes, reservedRoute => reservedRoute == routeValue.Key);
                    if (exists)
                    {
                        var resourceContextValue = routeValue.Value.ToString();

                        result.Fields[routeValue.Key] = Value.ForString(resourceContextValue);
                    }
                }
            }
            else
            {
                if (request is System.Web.HttpRequestWrapper)
                {
                    if (request.Params == null || request.Params.Count == 0)
                    {
                        return null;
                    }
                    foreach (var key in request.Params.Keys)
                    {
                        bool exists = Array.Exists(ReservedRoutes, reservedRoute => reservedRoute == key);
                        if (exists)
                        {
                            continue;
                        }

                        var resourceContextValue = request.Params[key].ToString();

                        result.Fields[key] = Value.ForString(resourceContextValue);
                    }
                }
                else
                {
                    if (request.Properties == null || request.Properties.Count == 0)
                    {
                        return null;
                    }
                    foreach (var props in request.Properties)
                    {
                        bool exists = Array.Exists(ReservedRoutes, reservedRoute => reservedRoute == props.Key);
                        if (exists)
                        {
                            continue;
                        }

                        var resourceContextValue = props.Value.ToString();

                        result.Fields[props.Key] = Value.ForString(resourceContextValue);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The default Identity Context mapper function.
        /// </summary>
        /// <param name="identity">The provided identity.</param>
        /// <param name="supportedClaimTypes">A list containing supported claims type.</param>
        /// <returns>The default Identity Context mapper.</returns>
        public static IdentityContext DefaultIdentityContext(ClaimsPrincipal identity, IEnumerable<string> supportedClaimTypes)
        {
            var identityContext = new IdentityContext();
            identityContext.Type = IdentityType.None;
            if (identity != null)
            {
                if (identity.Identity.AuthenticationType != null && identity.Identity != null)
                {
                    foreach (string supportedClaimType in supportedClaimTypes)
                    {
                        var claim = identity.FindFirst(c => c.Type == supportedClaimType);
                        if (claim != null)
                        {
                            if (claim.Value != ""){
                                identityContext.Type = IdentityType.Sub;
                                identityContext.Identity = claim.Value;
                                break;
                            }
                        }
                    }
                }
            }

            return identityContext;
        }
    }
}