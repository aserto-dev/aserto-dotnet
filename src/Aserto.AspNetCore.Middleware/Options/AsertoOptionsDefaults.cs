//-----------------------------------------------------------------------
// <copyright file="AsertoOptionsDefaults.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Options
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.AspNetCore.Http;

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
            var policyPath = policyRoot;

            policyPath = $"{policyPath}.{request.Method.ToUpper()}";
            policyPath = $"{policyPath}{request.Path.Value.Replace("/", ".").Replace(":", "__").ToLower()}".TrimEnd('.');

            Regex regex = new Regex("[^a-zA-Z0-9._]");
            policyPath = regex.Replace(policyPath, "_");

            return policyPath;
        }
    }
}
