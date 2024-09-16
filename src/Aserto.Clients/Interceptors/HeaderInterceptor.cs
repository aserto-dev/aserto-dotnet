//-----------------------------------------------------------------------
// <copyright file="HeaderInterceptor.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.Clients.Interceptors
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Header interceptor for gRPC calls.
    /// </summary>
    public class HeaderInterceptor : Interceptor
    {
        private readonly string apiKey;
        private readonly string tenantID;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderInterceptor"/> class.
        /// </summary>
        /// <param name="apiKey">The API key for authorization header.</param>
        /// <param name="tenantID">The tenant ID for header.</param>
        public HeaderInterceptor(string apiKey, string tenantID)
        {
            this.apiKey = apiKey;
            this.tenantID = tenantID;
        }

        /// <inheritdoc/>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = new Metadata();
            headers.Add(new Metadata.Entry("Aserto-Tenant-Id", tenantID));
            headers.Add(new Metadata.Entry("Authorization", $"basic {apiKey}"));

            var newOptions = context.Options.WithHeaders(headers);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);

            return base.AsyncUnaryCall(request, newContext, continuation);
        }
    }
}
