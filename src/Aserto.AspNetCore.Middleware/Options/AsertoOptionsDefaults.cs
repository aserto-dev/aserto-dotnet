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
    using Microsoft.AspNetCore.Routing.Patterns;

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
        /// Gets a value indicating the Aserto Authorizer API Key.
        /// </summary>
        public static string AuthorizerApiKey { get; } = string.Empty;

        /// <summary>
        /// Gets a value indicating the Aserto Tenant Identifier.
        /// </summary>
        public static string TenantID { get; } = string.Empty;

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
        public static string DefaultPolicyPathMapper(string policyRoot, HttpRequest request)
        {
            string policyPath;
            if (request.HttpContext == null || request.HttpContext.GetEndpoint() == null)
            {
                policyPath = request.Path;
            }
            else
            {
                policyPath = ParseRouteEndpoint(request);
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
            policyPath = $"{request.Method.ToUpper()}.{policyPath.TrimStart('.')}";

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
        public static Struct DefaultResourceMapper(string policyRoot, HttpRequest request)
        {
            if (request.RouteValues == null || request.RouteValues.Count == 0)
            {
                return null;
            }

            Struct result = new Struct();
            foreach (var routeValue in request.RouteValues)
            {
                bool exists = Array.Exists(ReservedRoutes, reservedRoute => reservedRoute == routeValue.Key);
                if (exists)
                {
                    continue;
                }

                var resourceContextValue = routeValue.Value.ToString();

                result.Fields[routeValue.Key] = Value.ForString(resourceContextValue);
            }

            return result;
        }

        private static string ParseRouteEndpoint(HttpRequest request)
        {
            var routeEndpoint = (RouteEndpoint)request.HttpContext.GetEndpoint();
            if (routeEndpoint == null)
            {
                return request.Path;
            }

            string result = string.Empty;

            var routePatternPieces = routeEndpoint.RoutePattern.RawText.Split("/");

            foreach (var piece in routePatternPieces)
            {
                // Not a parameter
                if (!piece.StartsWith("{"))
                {
                    result = $"{result}/{piece}";
                    continue;
                }

                // handle parameters that have default and that are nullable
                var processedPiece = piece.TrimStart('{').TrimEnd('}').Split('=')[0].Split('?')[0];

                // handle reserver routes
                bool isReserver = Array.Exists(ReservedRoutes, reservedRoute => processedPiece == reservedRoute);

                if (isReserver)
                {
                    result = $"{result}/{request.RouteValues[processedPiece]}";
                }
                else
                {
                    // handle optional parameters
                    if (request.RouteValues.ContainsKey(processedPiece))
                    {
                        result = $"{result}/{{{processedPiece}}}";
                    }
                }
            }

            return result;
        }
    }
}
