//-----------------------------------------------------------------------
// <copyright file="AsertoAuthorizerOptions.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.Clients.Options
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using System.Text;
    using Aserto.Authorizer.V2.Api;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Options for Aserto Authorizer Client.
    /// </summary>
    public class AsertoAuthorizerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating the Aserto Service URL.
        /// </summary>
        public string ServiceUrl { get; set; } = "https://localhost:8282";

        /// <summary>
        /// Gets or sets a value indicating the Aserto Authorizer API Key.
        /// </summary>
        public string AuthorizerApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Aserto Tenant ID.
        /// </summary>
        public string TenantID { get; set; } 

        /// <summary>
        /// Gets or sets a value indicating whether insecure service connections are allowed when using SSL.
        /// </summary>
        public bool Insecure { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether service connections are in plaintext.
        /// </summary>
        public bool PlainText { get; set; }

        /// <summary>
        /// Validates the provided options.
        /// </summary>
        /// <param name="options">Authorizer API Client options <see cref="Aserto.Clients.Options.AsertoAuthorizerOptions"/>.</param>
        /// <returns>true if the configuration is valid.</returns>
        public static bool Validate(AsertoAuthorizerOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!ValidateUri(options.ServiceUrl))
            {
                return false;
            }

            if (options.PlainText)
            {
                if (new Uri(options.ServiceUrl).Scheme != Uri.UriSchemeHttp)
                {
                    throw new ArgumentException("invalid service scheme for plain text client configuration");
                }
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