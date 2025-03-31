//-----------------------------------------------------------------------
// <copyright file="AsertoDirectoryOptions.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.Clients.Options
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Options for Aserto Directory Client.
    /// </summary>
    public class AsertoDirectoryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoDirectoryOptions"/> class.
        /// </summary>
        public AsertoDirectoryOptions()
        {
            // Empty constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsertoDirectoryOptions"/> class.
        /// </summary>
        /// <param name="serviceURL">Directory service url.</param>
        /// <param name="readerURL">Directory reader url.</param>
        /// <param name="writerURL">Directory writer url.</param>
        /// <param name="importerURL">Directory importer url.</param>
        /// <param name="exporterURL">Directory exporter url.</param>
        /// <param name="modelURL">Directory model url.</param>
        /// <param name="apiKey">Directory service API Key.</param>
        /// <param name="tenantID">Directory service tenant ID.</param>
        /// <param name="insecure">Bool indicating whether insecure service connections are allowed when using SSL.</param>
        public AsertoDirectoryOptions(string serviceURL = "", string readerURL = "", string writerURL = "", string importerURL = "", string exporterURL = "", string modelURL = "", string apiKey = "", string tenantID = "", bool insecure = false)
        {
            if (serviceURL != string.Empty)
            {
                this.DirectoryServiceUrl = serviceURL;
            }
            else
            {
                this.DirectoryReaderUrl = readerURL;
                this.DirectoryWriterUrl = writerURL;
                this.DirectoryImporterUrl = importerURL;
                this.DirectoryExporterUrl = exporterURL;
                this.DirectoryModelUrl = modelURL;
                this.DirectoryApiKey = apiKey;
            }

            if (tenantID != string.Empty)
            {
                this.DirectoryTenantID = tenantID;
            }

            this.DirectoryInsecure = insecure;
        }

        /// <summary>
        /// Gets or sets a value indicating the Directory Service URL.
        /// </summary>
        public string DirectoryServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Directory reader URL.
        /// </summary>
        public string DirectoryReaderUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Directory writer URL.
        /// </summary>
        public string DirectoryWriterUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Directory importer URL.
        /// </summary>
        public string DirectoryImporterUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Directory exporter URL.
        /// </summary>
        public string DirectoryExporterUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Directory model URL.
        /// </summary>
        public string DirectoryModelUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Aserto Directory API Key.
        /// </summary>
        public string DirectoryApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Aserto Tenant ID.
        /// </summary>
        public string DirectoryTenantID { get; set; } 

        /// <summary>
        /// Gets or sets a value indicating whether insecure service connections are allowed when using SSL.
        /// </summary>
        public bool DirectoryInsecure { get; set; } 

        /// <summary>
        /// Validates the provided options.
        /// </summary>
        /// <param name="options">Directory API Client options <see cref="AsertoDirectoryOptions"/>.</param>
        internal static void Validate(AsertoDirectoryOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.DirectoryServiceUrl) && string.IsNullOrEmpty(options.DirectoryReaderUrl) &&
            string.IsNullOrEmpty(options.DirectoryWriterUrl) && string.IsNullOrEmpty(options.DirectoryImporterUrl) &&
            string.IsNullOrEmpty(options.DirectoryExporterUrl) && string.IsNullOrEmpty(options.DirectoryModelUrl))
            {
                throw new ArgumentException("no url provided for directory services");
            }

            if (!string.IsNullOrEmpty(options.DirectoryServiceUrl) && !ValidateUri(options.DirectoryServiceUrl))
            {
                throw new ArgumentException("wrong url provided for directory service url");
            }

            if (!string.IsNullOrEmpty(options.DirectoryReaderUrl) && !ValidateUri(options.DirectoryReaderUrl))
            {
                throw new ArgumentException("wrong url provided for directory reader service");
            }

            if (!string.IsNullOrEmpty(options.DirectoryWriterUrl) && !ValidateUri(options.DirectoryWriterUrl))
            {
                throw new ArgumentException("wrong url provided for directory writer service");
            }

            if (!string.IsNullOrEmpty(options.DirectoryImporterUrl) && !ValidateUri(options.DirectoryImporterUrl))
            {
                throw new ArgumentException("wrong url provided for directory importer service");
            }

            if (!string.IsNullOrEmpty(options.DirectoryExporterUrl) && !ValidateUri(options.DirectoryExporterUrl))
            {
                throw new ArgumentException("wrong url provided for directory exporter service");
            }

            if (!string.IsNullOrEmpty(options.DirectoryModelUrl) && !ValidateUri(options.DirectoryModelUrl))
            {
                throw new ArgumentException("wrong url provided for directory model service");
            }

            return;
        }

        private static bool ValidateUri(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                return false;
            }

            var serviceUri = new Uri(uri);

            if (serviceUri.Scheme != Uri.UriSchemeHttps && serviceUri.Scheme != Uri.UriSchemeHttp)
            {
                return false;
            }

            return true;
        }
    }
}