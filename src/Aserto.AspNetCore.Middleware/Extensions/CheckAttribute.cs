//-----------------------------------------------------------------------
// <copyright file="CheckAttribute.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Extensions
{
    using System;
    using Aserto.AspNetCore.Middleware.Options;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;

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

            this.ResourceMapperName = resourceMapperName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckAttribute"/> class.
        /// </summary>
        /// <param name="objectID">The id of the object to be included in the resource context.</param>
        /// <param name="objectType">The type of the object to be included in the resource context.</param>
        /// <param name="relationName">The name of the relation that authorizer to check against.</param>
        public CheckAttribute(string objectID, string objectType, string relationName)
        {
            this.ResourceMapper = (policyRoot, httpRequest) =>
            {
                Struct result = new Struct();
                result.Fields.Add("object_id", Value.ForString(objectID));
                result.Fields.Add("object_type", Value.ForString(objectType));
                result.Fields.Add("relation", Value.ForString(relationName));

                return result;
            };
        }

        /// <summary>
        /// Gets or sets the name of the resource mapper used by the check attribute.
        /// </summary>
        public string ResourceMapperName { get => this.resourceMapperName; set => this.resourceMapperName = value; }

        /// <summary>
        /// Gets or sets the resource mapper used by the check attribute.
        /// </summary>
        public Func<string, HttpRequest, Struct> ResourceMapper { get; set; }
    }
}
