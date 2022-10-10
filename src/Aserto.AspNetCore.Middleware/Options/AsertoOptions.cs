//-----------------------------------------------------------------------
// <copyright file="AsertoOptions.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Options
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Options for Aserto Middleware.
    /// </summary>
    public class AsertoOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Aserto Authorization is enabled.
        /// </summary>
        public bool Enabled { get; set; } = AsertoOptionsDefaults.Enabled;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Service URL.
        /// </summary>
        public string ServiceUrl { get; set; } = AsertoOptionsDefaults.ServiceUrl;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Authorizer API Key.
        /// </summary>
        public string AuthorizerApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Aserto Tenant ID.
        /// </summary>
        public string TenantID { get; set; } = AsertoOptionsDefaults.TenantID;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Policy ID.
        /// </summary>
        public string PolicyName { get; set; } = AsertoOptionsDefaults.PolicyName;

        /// <summary>
        /// Gets or sets a value indicating whether insecure service connections are allowed when using SSL.
        /// </summary>
        public bool Insecure { get; set; } = AsertoOptionsDefaults.Insecure;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Policy ID.
        /// </summary>
        public string PolicyInstanceLabel { get; set; } = AsertoOptionsDefaults.PolicyInstanceLabel;

        /// <summary>
        /// Gets or sets a value indicating the decision string to be used.
        /// </summary>
        public string Decision { get; set; } = AsertoOptionsDefaults.Decision;

        /// <summary>
        /// Gets or sets the URL to Policy mapper.
        /// </summary>
        public Func<HttpRequest, string> PolicyPathMapper { get; set; } = AsertoOptionsDefaults.DefaultPolicyPathMapper;

        /// <summary>
        /// Gets or sets the Resource mapper.
        /// </summary>
        public Func<HttpRequest, Struct> ResourceMapper { get; set; } = AsertoOptionsDefaults.DefaultResourceMapper;

        /// <summary>
        /// Validates the provided options.
        /// </summary>
        /// <param name="options">Authorizer API Client options <see cref="AsertoOptions"/>.</param>
        /// <returns>true if the configuration is valid.</returns>
        internal static bool Validate(AsertoOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!ValidateUri(options.ServiceUrl))
            {
                return false;
            }

            return true;
        }

        private static bool ValidateUri(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                return false;
            }

            var serviceUri = new Uri(uri);

            if (serviceUri.Scheme != Uri.UriSchemeHttps && serviceUri.Scheme != Uri.UriSchemeHttp)
            {
                return false;
            }

            return true;
        }
    }
}
