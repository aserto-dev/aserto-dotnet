//-----------------------------------------------------------------------
// <copyright file="AsertoServicesExtensions.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Aserto.AspNetCore.Middleware.Clients;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.AspNetCore.Middleware.Policies;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Extension methods for Aserto Authorization.
    /// </summary>
    public static class AsertoServicesExtensions
    {
        /// <summary>
        /// Adds services and options for the Aserto authorization middleware.
        /// </summary>
        /// <param name="services">/>The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="configure">An action delegate to configure the provided Aserto Options.</param>
        /// <param name="authorizerConfig">An action delegate to configure the authorizer client.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAsertoAuthorization(this IServiceCollection services, Action<AsertoOptions> configure, Action<AsertoAuthorizerOptions> authorizerConfig)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.AddOptions<AsertoOptions>().Configure(configure).Validate(AsertoOptions.Validate);
            services.AddOptions<AsertoAuthorizerOptions>().Configure(authorizerConfig).Validate(AsertoAuthorizerOptions.Validate);
            services.TryAddSingleton<IAuthorizerAPIClient, AuthorizerAPIClient>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.TryAddSingleton<IAuthorizationHandler, AsertoAuthorizationHandler>();
            return services;
        }

        /// <summary>
        /// Adds services and options for the Aserto authorization middleware.
        /// </summary>
        /// <param name="services">/>The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="checkOptions">An action delegate to configure the provided Check Options.</param>
        /// <param name="authorizerConfig">An action delegate to configure the authorizer client.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAsertoCheckAuthorization(this IServiceCollection services, CheckOptions checkOptions, Action<AsertoAuthorizerOptions> authorizerConfig)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (checkOptions == null)
            {
                throw new ArgumentNullException(nameof(checkOptions));
            }

            services.AddOptions<AsertoAuthorizerOptions>().Configure(authorizerConfig).Validate(AsertoAuthorizerOptions.Validate);
            services.AddOptions<CheckOptions>().Configure(check =>
            {
                check.ResourceMappingRules = checkOptions.ResourceMappingRules;
                check.BaseOptions = checkOptions.BaseOptions;
                check.ObjectMappingRules = checkOptions.ObjectMappingRules;
            });
            services.TryAddSingleton<IAuthorizerAPIClient, AuthorizerAPIClient>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.TryAddSingleton<IAuthorizationHandler, CheckAuthorizationHandler>();
            return services;
        }
    }
}
