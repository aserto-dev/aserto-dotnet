//-----------------------------------------------------------------------
// <copyright file="AsertoOptionsDefaults.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Options
{
    using System;
    using System.Text.RegularExpressions;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    /// <summary>
    /// Defaults for Aserto Options.
    /// </summary>
    public static class AsertoOptionsDefaults
    {
        /// <summary>
        /// Gets a value indicating whether the Aserto authorization is enabled.
        /// </summary>
        public static bool Enabled { get; } = true;

        /// <summary>
        /// Gets a value indicating the Aserto service URL.
        /// </summary>
        public static string ServiceUrl { get; } = "https://authorizer.prod.aserto.com:8443";

        /// <summary>
        /// Gets a value indicating the Aserto Authorizer API Key.
        /// </summary>
        public static string AuthorizerApiKey { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the Aserto Tenant Identifier.
        /// </summary>
        public static string TenantID { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the Aserto Policy ID.
        /// </summary>
        public static string PolicyID { get; } = string.Empty;

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
        internal static string DefaultPolicyPathMapper(string policyRoot, HttpRequest request)
        {
            string policyPath;
            if (request.HttpContext == null || request.HttpContext.GetEndpoint() == null)
            {
                policyPath = request.Path;
            }
            else
            {
                var routeEndpoint = (RouteEndpoint)request.HttpContext.GetEndpoint();
                if (routeEndpoint != null)
                {
                    policyPath = routeEndpoint.RoutePattern.RawText;
                }
                else
                {
                    policyPath = request.Path;
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
            policyPath = $"{policyRoot}.{request.Method.ToUpper()}.{policyPath.TrimStart('.')}";

            // Trim tailing dots
            policyPath = policyPath.TrimEnd('.');

            Regex regex = new Regex("[^a-zA-Z0-9._]");
            policyPath = regex.Replace(policyPath, "_");

            return policyPath;
        }

        /// <summary>
        /// The default Resource Context mapper function.
        /// </summary>
        /// <param name="policyRoot">The policy root.</param>
        /// <param name="request">The <see cref="HttpRequest"/>.</param>
        /// <returns>The default Resource Context mapper.</returns>
        internal static Struct DefaultResourceMapper(string policyRoot, HttpRequest request)
        {
            if (request.RouteValues == null || request.RouteValues.Count == 0)
            {
                return null;
            }

            Struct result = new Struct();
            foreach (var routeValue in request.RouteValues)
            {
                // Skip reserved routing names: https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#reserved-routing-names
                string[] reservedRoutes = { "action", "area", "controller", "handler", "page" };

                bool exists = Array.Exists(reservedRoutes, reservedRoute => reservedRoute == routeValue.Key);
                if (exists)
                {
                    continue;
                }

                var resourceContextValue = routeValue.Value.ToString();

                result.Fields[routeValue.Key] = Value.ForString(resourceContextValue);
            }

            return result;
        }
    }
}
