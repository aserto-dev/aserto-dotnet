//-----------------------------------------------------------------------
// <copyright file="AsertoDirectoryOptions.cs" company="Aserto Inc">
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
    /// Options for Aserto Directory Client.
    /// </summary>
    public class AsertoDirectoryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoDirectoryOptions"/> class.
        /// </summary>
        public AsertoDirectoryOptions()
        {
            // Empty constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoDirectoryOptions"/> class.
        /// </summary>
        /// <param name="serviceURL">Directory service url.</param>
        /// <param name="apiKey">Directory service API Key.</param>
        /// <param name="tenantID">Directory service tenant ID.</param>
        /// <param name="insecure">Bool indicating whether insecure service connections are allowed when using SSL.</param>
        public AsertoDirectoryOptions(string serviceURL, string apiKey, string tenantID, bool insecure)
        {
            if (serviceURL != string.Empty)
            {
                this.DirectoryServiceUrl = serviceURL;
            }

            this.DirectoryApiKey = apiKey;

            if (tenantID != string.Empty)
            {
                this.DirectoryTenantID = tenantID;
            }

            this.DirectoryInsecure = insecure;
        }

        /// <summary>
        /// Gets or sets a value indicating the Directory Service URL.
        /// </summary>
        public string DirectoryServiceUrl { get; set; } = AsertoOptionsDefaults.DirectoryServiceUrl;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Directory API Key.
        /// </summary>
        public string DirectoryApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Aserto Tenant ID.
        /// </summary>
        public string DirectoryTenantID { get; set; } = AsertoOptionsDefaults.DirectoryTenantID;

        /// <summary>
        /// Gets or sets a value indicating whether insecure service connections are allowed when using SSL.
        /// </summary>
        public bool DirectoryInsecure { get; set; } = AsertoOptionsDefaults.Insecure;

        /// <summary>
        /// Validates the provided options.
        /// </summary>
        /// <param name="options">Directory API Client options <see cref="AsertoDirectoryOptions"/>.</param>
        /// <returns>true if the configuration is valid.</returns>
        internal static bool Validate(AsertoDirectoryOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!ValidateUri(options.DirectoryServiceUrl))
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
