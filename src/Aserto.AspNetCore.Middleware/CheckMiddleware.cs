//-----------------------------------------------------------------------
// <copyright file="CheckMiddleware.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Aserto.AspNetCore.Middleware.Clients;
    using Aserto.AspNetCore.Middleware.Extensions;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Authorizer.V2;
    using Aserto.Authorizer.V2.API;
    using Aserto.Directory.Reader.V3;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Middleware used for Aserto Check Authorization.
    /// </summary>
    public class CheckMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly IOptionsMonitor<CheckOptions> optionsMonitor;
        private CheckOptions options;
        private IAuthorizerAPIClient client;
        private Dictionary<string, Func<string, HttpRequest, Struct>> resourceMappingRules = new Dictionary<string, Func<string, HttpRequest, Struct>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next request delegate.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="optionsMonitor">The options for the Aserto Middleware.</param>
        /// <param name="client">The <see cref="IAuthorizerAPIClient"/>.</param>
        public CheckMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptionsMonitor<CheckOptions> optionsMonitor, IAuthorizerAPIClient client)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.logger = loggerFactory.CreateLogger<AsertoMiddleware>();

            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            this.options = optionsMonitor.CurrentValue;
            this.optionsMonitor.OnChange(options =>
            {
                // Clear the cached settings so the next EnsuredConfigured will re-evaluate.
                this.options = options;
            });
            this.client = client;
            this.resourceMappingRules = this.options.ResourceMappingRules;

            if (this.options.BaseOptions.PolicyPathMapper == null)
            {
                this.options.BaseOptions.PolicyPathMapper = this.DefaultCheckPolicyPathMapper;
            }
        }

        /// <summary>
        /// Process requests.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The task.</returns>
        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var checkAttribute = endpoint.Metadata.GetMetadata<Extensions.CheckAttribute>();
                if (checkAttribute != null)
                {
                    this.ApplyOptionsFromAttribute(context, checkAttribute);
                    var request = this.client.BuildIsRequest(context, Utils.DefaultClaimTypes, this.options.BaseOptions);

                    var allowed = await this.client.IsAsync(request);
                    if (!allowed && this.options.BaseOptions.Enabled)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        var errorMessage = Encoding.UTF8.GetBytes(HttpStatusCode.Forbidden.ToString());
                        await context.Response.Body.WriteAsync(errorMessage, 0, errorMessage.Length);
                    }
                    else
                    {
                        this.logger.LogInformation($"Decision to allow: {context.Request.Path} was: {allowed}");
                        await this.next.Invoke(context);
                    }
                }
                else
                {
                    this.logger.LogInformation($"Endpoint information for: {context.Request.Path} does not have check attribute - allowing request");
                    await this.next.Invoke(context);
                }
            }
            else
            {
                this.logger.LogInformation($"Endpoint information for: {context.Request.Path} is null - allowing request");
                await this.next.Invoke(context);
            }
        }

        /// <summary>
        /// Applies the resource mapper and/or the object mapper from check attribute to middleware options.
        /// </summary>
        /// <param name="context">The Http context.</param>
        /// <param name="attribute">The check attribute object.</param>
        private void ApplyOptionsFromAttribute(HttpContext context, Extensions.CheckAttribute attribute)
        {
            var obj = new CheckParams();
            if (!string.IsNullOrEmpty(attribute.ObjectMapperName))
            {
                Func<string, HttpRequest, CheckParams> objMapper = null;
                this.options.ObjectMappingRules.TryGetValue(attribute.ObjectMapperName, out objMapper);
                obj = objMapper(this.options.BaseOptions.PolicyRoot, context.Request);
            }

            Func<string, HttpRequest, Struct> resourceMapper = null;
            if (attribute.ResourceMapperName != null)
            {
                this.resourceMappingRules.TryGetValue(attribute.ResourceMapperName, out resourceMapper);
            }
            else if (attribute.ResourceMapper != null)
            {
                resourceMapper = attribute.ResourceMapper;
            }

            if (!string.IsNullOrEmpty(obj.SubjectType) && obj.SubjectType != "user")
            {
                this.options.BaseOptions.IdentityMapper = (identity, supportedClaimTypes) =>
                {
                    var identityContext = new IdentityContext();
                    identityContext.Type = IdentityType.Manual;
                    identityContext.Identity = obj.SubjectID;

                    return identityContext;
                };
            }

            this.options.BaseOptions.ResourceMapper = (policyRoot, httpRequest) =>
            {
                Struct result = new Struct();
                var resourceMapperValues = resourceMapper(policyRoot, httpRequest);

                string objID = (!string.IsNullOrEmpty(obj.ObjectID)) ? obj.ObjectID : resourceMapperValues.Fields["object_id"].StringValue;
                string objType = (!string.IsNullOrEmpty(obj.ObjectType)) ? obj.ObjectType : resourceMapperValues.Fields["object_type"].StringValue;
                string relation = (!string.IsNullOrEmpty(obj.Relation)) ? obj.Relation : resourceMapperValues.Fields["relation"].StringValue;

                result.Fields.Add("object_type", Value.ForString(objType));
                result.Fields.Add("relation", Value.ForString(relation));
                result.Fields.Add("object_id", Value.ForString(objID));
                if (!string.IsNullOrEmpty(obj.SubjectType))
                {
                    result.Fields.Add("subject_type", Value.ForString(obj.SubjectType));
                }

                return result;
            };
        }

        /// <summary>
        /// Default policy mapper for Check Middleware.
        /// </summary>
        /// <param name="policyRoot">The policy root.</param>
        /// <param name="request">The Incoming Http request.</param>
        /// <returns>The policy path.</returns>
        private string DefaultCheckPolicyPathMapper(string policyRoot, HttpRequest request)
        {
            return policyRoot + "." + "check";
        }
    }
}
