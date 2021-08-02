//-----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;

    /// <summary>
    /// Utility class.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Gets the default claim types to check.
        /// </summary>
        internal static IEnumerable<string> DefaultClaimTypes
        {
            get
            {
                IList<string> defaultClaimTypes = new List<string>
                {
                    ClaimTypes.Email, ClaimTypes.Name, ClaimTypes.NameIdentifier,
                };
                return defaultClaimTypes;
            }
        }
    }
}
