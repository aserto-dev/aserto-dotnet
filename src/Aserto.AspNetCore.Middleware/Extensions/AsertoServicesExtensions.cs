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
        /// <param name="section">The service configuration section<see cref="IConfigurationSection"/>.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAsertoAuthorization(this IServiceCollection services, IConfigurationSection section)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            services.AddOptions<AsertoOptions>().Bind(section).Validate(AsertoOptions.Validate);
            services.TryAddSingleton<IAuthorizerAPIClient, AuthorizerAPIClient>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.TryAddSingleton<IAuthorizationHandler, AsertoAuthorizationHandler>();
            return services;
        }
    }
}
