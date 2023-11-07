//-----------------------------------------------------------------------
// <copyright file="CheckAttribute.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Extensions
{
    using Aserto.AspNetCore.Middleware.Options;

    /// <summary>
    /// Attribute that can be used with the Aserto Check Middleware Authorization.
    /// </summary>
    public class CheckAttribute : System.Attribute
    {
        private string resourceMapperName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckAttribute"/> class.
        /// </summary>
        /// <param name="resourceMapperName">The name of the resource mapper used with this call.</param>
        public CheckAttribute(string resourceMapperName)
        {
            if (string.IsNullOrEmpty(resourceMapperName))
            {
                throw new System.ArgumentException($"'{nameof(resourceMapperName)}' cannot be null or empty.", nameof(resourceMapperName));
            }

            this.ResourceMapper = resourceMapperName;
        }

        /// <summary>
        /// Gets or sets the name of the resource mapper used by the check attribute.
        /// </summary>
        public string ResourceMapper { get => this.resourceMapperName; set => this.resourceMapperName = value; }
    }
}
