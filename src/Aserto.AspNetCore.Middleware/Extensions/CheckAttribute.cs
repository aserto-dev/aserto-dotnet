﻿//-----------------------------------------------------------------------
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
        /// <param name="relation">The name of the relation that authorizer to check against.</param>
        /// <param name="objectIdFromPath">The http path parameter that will be retrieved as the object id.</param>
        /// <param name="customMapper"> The name of the object mapper to use for the Check call.</param>
        public CheckAttribute(string objectID = "", string objectType = "", string relation = "", string objectIdFromPath = "", string customMapper = "")
        {
            // validate params
            if (string.IsNullOrEmpty(objectID) && string.IsNullOrEmpty(objectIdFromPath) && string.IsNullOrEmpty(customMapper))
            {
                throw new ArgumentException("One of \"objectID\", \"objectIdFromPath\"  or \"customMapper\" must be provided");
            }

            if (!string.IsNullOrEmpty(customMapper))
            {
                this.ObjectMapperName = customMapper;
            }

            this.ResourceMapper = (policyRoot, httpRequest) =>
            {
                Struct result = new Struct();
                result.Fields.Add("object_type", Value.ForString(objectType));
                result.Fields.Add("relation", Value.ForString(relation));
                if (string.IsNullOrEmpty(objectIdFromPath))
                {
                    result.Fields.Add("object_id", Value.ForString(objectID));
                }
                else
                {
                     result.Fields.Add("object_id", Value.ForString((string)httpRequest.RouteValues[objectIdFromPath]));
                }

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

        /// <summary>
        /// Gets or sets the object mapper used by the check attribute.
        /// </summary>
        public string ObjectMapperName { get; set; }
    }
}
