//-----------------------------------------------------------------------
// <copyright file="Directory.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Clients.Directory.V3
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Aserto.AspNetCore.Middleware.Clients;
    using Aserto.AspNetCore.Middleware.Options;
    using Aserto.Directory.Common.V3;
    using Aserto.Directory.Exporter.V3;
    using Aserto.Directory.Importer.V3;
    using Aserto.Directory.Model.V3;
    using Aserto.Directory.Reader.V3;
    using Aserto.Directory.Writer.V3;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using static Aserto.Directory.Exporter.V3.Exporter;
    using static Aserto.Directory.Importer.V3.Importer;
    using static Aserto.Directory.Model.V3.Model;
    using static Aserto.Directory.Reader.V3.Reader;
    using static Aserto.Directory.Writer.V3.Writer;
    using Object = Aserto.Directory.Common.V3.Object;
    using Relation = Aserto.Directory.Common.V3.Relation;

    /// <summary>
    /// Client for Aserto Directory API.
    /// </summary>
    public class Directory : IDirectory
    {
        private readonly ReaderClient readerClient;
        private readonly WriterClient writerClient;
        private readonly ImporterClient importerClient;
        private readonly ExporterClient exporterClient;
        private readonly ModelClient modelClient;
        private readonly AsertoDirectoryOptions options;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Directory"/> class.
        /// </summary>
        /// <param name="options">Authorizer API Client options <see cref="AsertoDirectoryOptions"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for this class.</param>
        public Directory(IOptions<AsertoDirectoryOptions> options, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<Directory>();

            if (options != null)
            {
                this.options = options.Value;
            }
            else
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!AsertoDirectoryOptions.Validate(this.options))
            {
                throw new ArgumentException("wrong url provided for DirectoryServiceUrl");
            }

            var grpcChannelOptions = new GrpcChannelOptions { };

            if (this.options.DirectoryInsecure)
            {
                var httpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                };

                grpcChannelOptions = new GrpcChannelOptions { HttpHandler = httpHandler };
            }

            var interceptor = new HeaderInterceptor(this.options.DirectoryApiKey, this.options.DirectoryTenantID);

            var channel = GrpcChannel.ForAddress(
                this.options.DirectoryServiceUrl,
                grpcChannelOptions);

            var invoker = channel.Intercept(interceptor);

            this.readerClient = new ReaderClient(invoker);
            this.writerClient = new WriterClient(invoker);
            this.importerClient = new ImporterClient(invoker);
            this.exporterClient = new ExporterClient(invoker);
            this.modelClient = new ModelClient(invoker);
        }

        /// <summary>
        /// Creates an <see cref="Object"/> use for the directory calls.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="id">The id of the object.</param>
        /// <param name="displayName">The display name of the object.</param>
        /// <param name="properties">A struct representing the properties bag of the object.</param>
        /// <param name="hash">The hash of the object.</param>
        /// <returns>A new <see cref="Object"/>.</returns>
        public static Object BuildObject(string type, string id, string displayName = "", Struct properties = null, string hash = "")
        {
            var obj = new Object();
            obj.Id = id;
            obj.Type = type;
            obj.DisplayName = displayName;
            obj.Properties = properties;
            obj.Etag = hash;

            return obj;
        }

        /// <summary>
        /// Creates an <see cref="Relation"/> use for the directory calls.
        /// </summary>
        /// <param name="objectType">The type of the object of the relation.</param>
        /// <param name="objectId">The ID of the object of the relation.</param>
        /// <param name="relationName">The type name of the relation.</param>
        /// <param name="subjectType">The type of the subject of the relation.</param>
        /// <param name="subjectId">The ID of the subject of the relation.</param>
        /// <param name="subjectRelation">Optional: The relation of the subject of the relation.</param>
        /// <returns>A new <see cref="Relation"/>.</returns>
        public static Relation BuildRelation(string objectType, string objectId, string relationName, string subjectType, string subjectId, string subjectRelation = "")
        {
            var relation = new Relation();
            relation.SubjectId = subjectId;
            relation.SubjectType = subjectType;
            relation.SubjectRelation = subjectRelation;
            relation.ObjectId = objectId;
            relation.ObjectType = objectType;
            relation.Relation_ = relationName;

            return relation;
        }

        /// <summary>
        /// Creates a <see cref="PaginationRequest"/> use for the directory calls.
        /// </summary>
        /// <param name="size">The number of items per page.</param>
        /// <param name="token">The token representing the page from which to start reading.</param>
        /// <returns>A new <see cref="PaginationRequest"/>.</returns>
        public static PaginationRequest BuildPaginationRequest(int size, string token)
        {
            var page = new PaginationRequest();
            page.Size = size;
            page.Token = token;

            return page;
        }

        /// <inheritdoc/>
        public async Task<GetObjectResponse> GetObjectAsync(string type, string id, bool withRelations = false)
        {
            var req = new GetObjectRequest();
            req.ObjectId = id;
            req.ObjectType = type;
            req.WithRelations = withRelations;
            var result = await this.readerClient.GetObjectAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<GetObjectsResponse> GetObjectsAsync(string type, int pageSize = 50, string pageToken = "")
        {
            var req = new GetObjectsRequest();
            var page = BuildPaginationRequest(pageSize, pageToken);
            req.ObjectType = type;
            req.Page = page;
            var result = await this.readerClient.GetObjectsAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<GetRelationResponse> GetRelationAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", string subjectRelation = "", bool withObjects = false)
        {
            var req = new GetRelationRequest();
            req.SubjectId = subjectId;
            req.SubjectRelation = subjectRelation;
            req.SubjectType = subjectType;
            req.Relation = relationName;
            req.WithObjects = withObjects;
            req.ObjectId = objId;
            req.ObjectType = objType;
            req.WithObjects = withObjects;
            var result = await this.readerClient.GetRelationAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<GetRelationsResponse> GetRelationsAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", string subjectRelation = "", bool withObjects = false, int pageSize = 50, string pageToken = "")
        {
            var page = BuildPaginationRequest(pageSize, pageToken);
            var req = new GetRelationsRequest();
            req.SubjectId = subjectId;
            req.SubjectType = subjectType;
            req.SubjectRelation = subjectRelation;
            req.ObjectId = objId;
            req.ObjectType = objType;
            req.WithObjects = withObjects;
            req.Relation = relationName;
            req.Page = page;
            var result = await this.readerClient.GetRelationsAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<CheckPermissionResponse> CheckPermissionAsync(string objType = "", string objId = "", string permissionName = "", string subjectType = "", string subjectId = "", bool trace = false)
        {
            var req = new CheckPermissionRequest();
            req.SubjectId = subjectId;
            req.SubjectType = subjectType;
            req.ObjectId = objId;
            req.ObjectType = objType;
            req.Permission = permissionName;
            req.Trace = trace;
            var result = await this.readerClient.CheckPermissionAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<CheckRelationResponse> CheckRelationAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", bool trace = false)
        {
            var req = new CheckRelationRequest();
            req.SubjectId = subjectId;
            req.SubjectType = subjectType;
            req.ObjectId = objId;
            req.ObjectType = objType;
            req.Relation = relationName;
            req.Trace = trace;
            var result = await this.readerClient.CheckRelationAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<CheckResponse> CheckAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", bool trace = false)
        {
            var req = new CheckRequest();
            req.SubjectId = subjectId;
            req.SubjectType = subjectType;
            req.ObjectId = objId;
            req.ObjectType = objType;
            req.Trace = trace;
            req.Relation = relationName;

            var result = await this.readerClient.CheckAsync(req);
            return result;
        }

        /// <inheritdoc/>
        public async Task<SetObjectResponse> SetObjectAsync(string type, string id, string displayName = "", Struct properties = null, string hash = "")
        {
            var req = new SetObjectRequest();
            req.Object = BuildObject(type, id, displayName, properties, hash);
            var result = await this.writerClient.SetObjectAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<DeleteObjectResponse> DeleteObjectAsync(string type, string id, bool withRelations = false)
        {
            var req = new DeleteObjectRequest();
            req.ObjectId = id;
            req.ObjectType = type;
            req.WithRelations = withRelations;
            var result = await this.writerClient.DeleteObjectAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<SetRelationResponse> SetRelationAsync(string objType, string objId, string relationName, string subjectType, string subjectId, string subjectRelation, string hash = "")
        {
            var req = new SetRelationRequest();
            req.Relation = BuildRelation(objType, objId, relationName, subjectType, subjectId, subjectRelation);
            var result = await this.writerClient.SetRelationAsync(req);

            return result;
        }

        /// <inheritdoc/>
        public async Task<DeleteRelationResponse> DeleteRelationAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", string subjectRelation = "")
        {
            var req = new DeleteRelationRequest();
            req.Relation = relationName;
            req.SubjectId = subjectId;
            req.SubjectType = subjectType;
            req.SubjectRelation = subjectRelation;
            req.ObjectId = objId;
            req.ObjectType = objType;
            var result = await this.writerClient.DeleteRelationAsync(req);

            return result;
        }

        /// <summary>
        /// Returns a directory manifest.
        /// </summary>
        /// <param name="request">Get manifest request parameter.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<GetManifestResponse> GetManifestAsync(GetManifestRequest request)
        {
            var response = new GetManifestResponse();

            var stream = this.modelClient.GetManifest(request);
            while (await stream.ResponseStream.MoveNext())
            {
                if (stream.ResponseStream.Current.Metadata != null)
                {
                    response.Metadata = stream.ResponseStream.Current.Metadata;
                }

                if (stream.ResponseStream.Current.Body != null)
                {
                    if (response.Body == null)
                    {
                        response.Body = stream.ResponseStream.Current.Body;
                    }
                    else
                    {
                        response.Body.MergeFrom(stream.ResponseStream.Current.Body);
                    }
                }

                if (stream.ResponseStream.Current.Model != null)
                {
                    response.Model.MergeFrom(stream.ResponseStream.Current.Model);
                }
            }

            return response;
        }

        /// <summary>
        /// Sets a directory manifest.
        /// </summary>
        /// <param name="request">The set manifest request.</param>
        /// <returns>Returns an async streaming call to set the manifest.</returns>
        public async Task<SetManifestResponse> SetManifestAsync(SetManifestRequest request)
        {
            var response = new SetManifestResponse();

            var stream = this.modelClient.SetManifest();
            await stream.RequestStream.WriteAsync(request);
            await stream.RequestStream.CompleteAsync();
            var responseTask = await stream.ResponseAsync;

            responseTask.MergeFrom(response);
            return response;
        }

        /// <summary>
        /// Delete the directory manifest.
        /// </summary>
        /// <param name="request">The delete manifest request.</param>
        /// <returns>Deletes the directory manifest.</returns>
        public async Task<DeleteManifestResponse> DeleteManifestAsync(DeleteManifestRequest request)
        {
            var result = await this.modelClient.DeleteManifestAsync(request);
            return result;
        }

        /// <summary>
        /// Imports data into a directory.
        /// </summary>
        /// <param name="request">The import data request.</param>
        /// <returns>Returns an async enumerable of import response data.</returns>
        public async IAsyncEnumerable<ImportResponse> ImportAsync(ImportRequest request)
        {
            var duplex = this.importerClient.Import();

            await duplex.RequestStream.WriteAsync(request);
            await duplex.RequestStream.CompleteAsync();

            var results = duplex.ResponseStream.ReadAllAsync();
            await foreach (var response in results)
            {
                yield return response;
            }
        }

        /// <summary>
        /// Exports data from a directory.
        /// </summary>
        /// <param name="request">Export request parameter.</param>
        /// <returns>Returns an async streaming call to export data.</returns>
        public async IAsyncEnumerable<ExportResponse> ExportAsync(ExportRequest request)
        {
           var stream = this.exporterClient.Export(request);
           while (await stream.ResponseStream.MoveNext())
           {
                var response = stream.ResponseStream.Current;
                yield return response;
            }
        }
    }
}
