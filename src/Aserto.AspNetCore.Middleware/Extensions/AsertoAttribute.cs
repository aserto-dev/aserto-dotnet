//-----------------------------------------------------------------------
// <copyright file="AsertoAttribute.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Extensions
{
    using Aserto.AspNetCore.Middleware.Options;

    /// <summary>
    /// Attribute that can be used with the Aserto Middleware Authorization.
    /// </summary>
    public class AsertoAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoAttribute"/> class.
        /// </summary>
        public AsertoAttribute()
        {
        }
    }
}
