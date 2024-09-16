//-----------------------------------------------------------------------
// <copyright file="IAuthorizerAPIClient.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.Clients.Authorizer
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Aserto.Authorizer.V2;
    using Aserto.Authorizer.V2.Api;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Client for communicating with the Aserto Authorizer API.
    /// </summary>
    public interface IAuthorizerAPIClient
    {
        /// <summary>
        /// Determines if the HTTP request is allowed.
        /// </summary>
        /// <param name="isRequest">The <see cref="IsRequest"/> that will be used for the request.</param>
        /// <returns>A bool indicating if the request is allowed.</returns>
        Task<bool> IsAsync(IsRequest isRequest);

        /// <summary>
        /// List the details of the running policy instance.
        /// </summary>
        /// <param name="listRequest">The <see cref="ListPoliciesRequest"/> that will be used for the request.</param>
        /// <returns>A list policies response that contains the bundle module details.</returns>
        Task<List<Module>> ListPoliciesAsync(ListPoliciesRequest listRequest);

        /// <summary>
        /// Get the details of the a policy instance module.
        /// </summary>
        /// <param name="getRequest">The <see cref="GetPolicyRequest"/> that will be used for the request.</param>
        /// <returns>The bundle module details.</returns>
        Task<Module> GetPolicyAsync(GetPolicyRequest getRequest);

        /// <summary>
        /// Execute a query on a policy instance.
        /// </summary>
        /// <param name="queryRequest">The <see cref="QueryRequest"/> that will be used for the request.</param>
        /// <returns>Response of the query execution.</returns>
        Task<QueryResponse> QueryAsync(QueryRequest queryRequest);

        /// <summary>
        /// Execute a partial evaluation on a policy instance.
        /// </summary>
        /// <param name="compileRequest">The <see cref="CompileRequest"/> that will be used for the request.</param>
        /// <returns>Response of the compile execution.</returns>
        Task<CompileResponse> CompileAsync(CompileRequest compileRequest);


        /// <summary>
        /// Returns the decision tree response contains results for the module paths.
        /// </summary>
        /// <param name="decisionTreeRequest">The <see cref="DecisionTreeRequest"/> that will be used for the request.</param>
        /// <returns>A decision tree response containg the module path and path root details.</returns>
        Task<DecisionTreeResponse> DecisionTreeAsync(DecisionTreeRequest decisionTreeRequest);
    }
}
