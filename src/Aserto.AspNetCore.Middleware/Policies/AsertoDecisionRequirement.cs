//-----------------------------------------------------------------------
// <copyright file="AsertoDecisionRequirement.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Policies
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Requirements for Aserto Authorization Policy.
    /// </summary>
    public class AsertoDecisionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoDecisionRequirement"/> class.
        /// </summary>
        public AsertoDecisionRequirement()
            : this(Utils.DefaultClaimTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoDecisionRequirement"/> class.
        /// </summary>
        /// <param name="claimTypes">The claim types to check.</param>
        public AsertoDecisionRequirement(IEnumerable<string> claimTypes)
        {
            this.RequiredClaimTypes = claimTypes;
        }

        /// <summary>
        /// Gets the claim types to validate.
        /// </summary>
        internal IEnumerable<string> RequiredClaimTypes { get; }
    }
}
