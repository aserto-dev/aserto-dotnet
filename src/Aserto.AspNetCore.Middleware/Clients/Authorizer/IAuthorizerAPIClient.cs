//-----------------------------------------------------------------------
// <copyright file="IAuthorizerAPIClient.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Aserto.Authorizer.V2;
    using Aserto.Authorizer.V2.API;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Client for communicating with the Aserto Authorizer API.
    /// </summary>
    public interface IAuthorizerAPIClient
    {
        /// <summary>
        /// Gets the decision string that will be used for all the requests.
        /// </summary>
        string Decision { get; }

        /// <summary>
        /// Gets the policy name that will be used for all the requests.
        /// </summary>
        string PolicyName { get; }

        /// <summary>
        /// Gets the policy instance label that will be used for all the requests.
        /// </summary>
        string PolicyInstanceLabel { get; }

        /// <summary>
        /// Gets the policy root that will be used for all the requests.
        /// </summary>
        string PolicyRoot { get; }

        /// <summary>
        /// Gets the function that converts an <see cref="HttpRequest"/> to a policy path.
        /// </summary>
        public Func<string, HttpRequest, string> PolicyPathMapper { get; }

        /// <summary>
        /// Gets the function that receives an <see cref="HttpRequest"/> and a policy root and converts it to a resource context.
        /// </summary>
        public Func<string, HttpRequest, Struct> ResourceMapper { get; }

        /// <summary>
        /// Gets the function that receives an identity value and an identity type and converts it to an identity context.
        /// </summary>
        public Func<ClaimsPrincipal, IEnumerable<string>, IdentityContext> IdentityMapper { get; }

        /// <summary>
        /// Determines if the HTTP request is allowed.
        /// </summary>
        /// <param name="isRequest">The <see cref="IsRequest"/> that will be used for the request.</param>
        /// <returns>A bool indicating if the request is allowed.</returns>
        Task<bool> IsAsync(IsRequest isRequest);
    }
}
