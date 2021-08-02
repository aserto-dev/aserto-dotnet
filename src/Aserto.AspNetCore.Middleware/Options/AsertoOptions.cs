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
        public string PolicyID { get; set; } = AsertoOptionsDefaults.PolicyID;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Policy Root.
        /// </summary>
        public string PolicyRoot { get; set; } = AsertoOptionsDefaults.PolicyRoot;

        /// <summary>
        /// Gets or sets a value indicating the decision string to be used.
        /// </summary>
        public string Decision { get; set; } = AsertoOptionsDefaults.Decision;

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

            if (!Uri.IsWellFormedUriString(options.ServiceUrl, UriKind.Absolute))
            {
                return false;
            }

            var serviceUri = new Uri(options.ServiceUrl);

            if (serviceUri.Scheme != Uri.UriSchemeHttps && serviceUri.Scheme != Uri.UriSchemeHttp)
            {
                return false;
            }

            if (options.AuthorizerApiKey == string.Empty)
            {
                return false;
            }

            if (options.PolicyID == string.Empty)
            {
                return false;
            }

            if (options.PolicyRoot == string.Empty)
            {
                return false;
            }

            if (options.TenantID == string.Empty)
            {
                return false;
            }

            return true;
        }
    }
}
