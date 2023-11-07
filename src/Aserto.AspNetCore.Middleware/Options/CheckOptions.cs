// <copyright file="CheckOptions.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Options
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using System.Text;
    using Aserto.Authorizer.V2.API;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Options for Aserto Check Middleware.
    /// </summary>
    public class CheckOptions : AsertoOptions
    {
        /// <summary>
        /// Gets or sets the resource mapping rules used in check middleware.
        /// </summary>
        public Dictionary<string, Func<string, HttpRequest, Struct>> ResourceMappingRules { get; set; } = new Dictionary<string, Func<string, HttpRequest, Struct>>(StringComparer.OrdinalIgnoreCase);
    }
}
