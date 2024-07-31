//-----------------------------------------------------------------------
// <copyright file="AsertoOptions.cs" company="Aserto Inc">
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
    using Aserto.Clients.Options;
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
        /// Gets or sets a value indicating the Aserto Policy Name.
        /// </summary>
        public string PolicyName { get; set; } =AsertoOptionsDefaults.PolicyName;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Instance label.
        /// </summary>
        public string PolicyInstanceLabel { get; set; } = AsertoOptionsDefaults.PolicyInstanceLabel;

        /// <summary>
        /// Gets or sets a value indicating the Aserto Policy Root.
        /// </summary>
        public string PolicyRoot { get; set; } = AsertoOptionsDefaults.PolicyRoot;

        /// <summary>
        /// Gets or sets a value indicating the decision string to be used.
        /// </summary>
        public string Decision { get; set; } = AsertoOptionsDefaults.Decision;

        /// <summary>
        /// Gets or sets the URL to Policy mapper.
        /// </summary>
        public Func<string, HttpRequest, string> PolicyPathMapper { get; set; } = null;

        /// <summary>
        /// Gets or sets the Resource mapper.
        /// </summary>
        public Func<string, HttpRequest, Struct> ResourceMapper { get; set; } = null;

        /// <summary>
        /// Gets or sets the Identity mapper for the check middleware.
        /// </summary>
        public Func<ClaimsPrincipal, IEnumerable<string>, IdentityContext> IdentityMapper { get; set; } = AsertoOptionsDefaults.DefaultIdentityContext;

        /// <summary>
        /// Validates the provided options.
        /// </summary>
        /// <param name="options">Authorizer API Client options <see cref="AsertoOptions"/>.</param>
        /// <returns>true if the configuration is valid.</returns>
        public static bool Validate(AsertoOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.PolicyRoot))
            {
                return false;
            }

            return true;
        }
    }
}
