// <copyright file="CheckParams.cs" company="Aserto Inc">
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
    using Aserto.Authorizer.V2.Api;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Options for Aserto Check Middleware.
    /// </summary>
    public class CheckParams
    {
        /// <summary>
        /// Gets or sets the Object ID.
        /// </summary>
        public string ObjectID { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Object type.
        /// </summary>
        public string ObjectType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Relation name.
        /// </summary>
        public string Relation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Subject type.
        /// </summary>
        public string SubjectType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Subject ID.
        /// </summary>
        public string SubjectID { get; set; } = string.Empty;
    }
}
