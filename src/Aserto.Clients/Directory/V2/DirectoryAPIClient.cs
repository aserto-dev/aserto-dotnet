//-----------------------------------------------------------------------
// <copyright file="DirectoryAPIClient.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.Clients.Directory.V2
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Aserto.Clients.Options;
    using Aserto.Clients.Interceptors;
    using Aserto.Directory.Common.V2;
    using Aserto.Directory.Reader.V2;
    using Aserto.Directory.Writer.V2;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using Grpc.Net.Client;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using static Aserto.Directory.Reader.V2.Reader;
    using static Aserto.Directory.Writer.V2.Writer;

    /// <summary>
    /// Client for Aserto Directory API.
    /// </summary>
    public class DirectoryAPIClient : IDirectoryAPIClient
    {
        private readonly ReaderClient readerClient;
        private readonly WriterClient writerClient;
        private readonly AsertoDirectoryOptions options;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryAPIClient"/> class.
        /// </summary>
        /// <param name="options">Authorizer API Client options <see cref="AsertoDirectoryOptions"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for this class.</param>
        public DirectoryAPIClient(IOptions<AsertoDirectoryOptions> options, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DirectoryAPIClient>();

            if (options != null)
            {
                this.options = options.Value;
            }
            else
            {
                throw new ArgumentNullException(nameof(options));
            }

            AsertoDirectoryOptions.Validate(this.options);

            var grpcChannelOptions = new GrpcChannelOptions { };

            if (this.options.DirectoryInsecure)
            {
                var httpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                };

                grpcChannelOptions = new GrpcChannelOptions { HttpHandler = httpHandler };
            }

            var interceptor = new HeaderInterceptor(this.options.DirectoryApiKey, this.options.DirectoryTenantID);

            var channels = BuildGrpcChannels(this.options, grpcChannelOptions);

            GrpcChannel readerChannel = null;
            GrpcChannel writerChannel = null;
            GrpcChannel serviceChannel = null;
            if (!string.IsNullOrEmpty(this.options.DirectoryReaderUrl) && channels.TryGetValue(this.options.DirectoryReaderUrl, out readerChannel))
            {
                var invoker = readerChannel.Intercept(interceptor);
                readerClient = new ReaderClient(invoker);
            }
            else if (!string.IsNullOrEmpty(this.options.DirectoryServiceUrl) && channels.TryGetValue(this.options.DirectoryServiceUrl, out serviceChannel))
            {
                var invoker = serviceChannel.Intercept(interceptor);
                readerClient = new ReaderClient(invoker);
            }
            else
            {
                readerClient = null;
            }

            if (!string.IsNullOrEmpty(this.options.DirectoryWriterUrl) && channels.TryGetValue(this.options.DirectoryWriterUrl, out writerChannel))
            {
                var invoker = writerChannel.Intercept(interceptor);
                writerClient = new WriterClient(invoker);
            }
            else if (!string.IsNullOrEmpty(this.options.DirectoryServiceUrl) && channels.TryGetValue(this.options.DirectoryServiceUrl, out serviceChannel))
            {
                var invoker = serviceChannel.Intercept(interceptor);
                writerClient = new WriterClient(invoker);
            }
            else
            {
                writerClient = null;
            }
        }

        /// <summary>
        /// Creates an <see cref="Aserto.Directory.Common.V2.Object"/> use for the directory calls.
        /// </summary>
        /// <param name="key">The key of the object.</param>
        /// <param name="type">The type of the object.</param>
        /// <param name="displayName">The display name of the object.</param>
        /// <param name="properties">A struct representing the properties bag of the object.</param>
        /// <param name="hash">The hash of the object.</param>
        /// <returns>A new <see cref="Aserto.Directory.Common.V2.Object"/>.</returns>
        [Obsolete]
        public static Aserto.Directory.Common.V2.Object BuildObject(string key, string type, string displayName, Struct properties = null, string hash = "")
        {
            var obj = new Aserto.Directory.Common.V2.Object();
            obj.Key = key;
            obj.Type = type;
            obj.DisplayName = displayName;
            obj.Properties = properties;
            obj.Hash = hash;

            return obj;
        }

        /// <summary>
        /// Creates an <see cref="ObjectIdentifier"/> use for the directory calls.
        /// </summary>
        /// <param name="key">The key of the object.</param>
        /// <param name="type">The type of the object.</param>
        /// <returns>A new <see cref="ObjectIdentifier"/>.</returns>
        [Obsolete]
        public static ObjectIdentifier BuildObjectIdentifier(string key, string type)
        {
            var obj = new ObjectIdentifier();
            obj.Key = key;
            obj.Type = type;

            return obj;
        }

        /// <summary>
        /// Creates an <see cref="Relation"/> use for the directory calls.
        /// </summary>
        /// <param name="subject">The <see cref="ObjectIdentifier"/> subject of the relation.</param>
        /// <param name="obj">The <see cref="ObjectIdentifier"/> object of the relation.</param>
        /// <param name="relationTypeName">The type name of the relation.</param>
        /// <param name="hash">The hash of the object.</param>
        /// <returns>A new <see cref="Relation"/>.</returns>
        [Obsolete]
        public static Relation BuildRelation(ObjectIdentifier subject, ObjectIdentifier obj, string relationTypeName, string hash = "")
        {
            var relation = new Relation();
            relation.Subject = subject;
            relation.Object = obj;
            relation.Relation_ = relationTypeName;
            relation.Hash = hash;

            return relation;
        }

        /// <summary>
        /// Creates an <see cref="RelationTypeIdentifier"/> use for the directory calls.
        /// </summary>
        /// <param name="name">The name of the relation type.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>A new <see cref="RelationTypeIdentifier"/>.</returns>
        [Obsolete]
        public static RelationTypeIdentifier BuildRelationTypeIdentifier(string name, string objectType)
        {
            var relType = new RelationTypeIdentifier();
            relType.Name = name;
            relType.ObjectType = objectType;

            return relType;
        }

        /// <summary>
        /// Creates an <see cref="RelationIdentifier"/> use for the directory calls.
        /// </summary>
        /// <param name="subject">The <see cref="ObjectIdentifier"/> subject of the relation.</param>
        /// <param name="obj">The <see cref="ObjectIdentifier"/> object of the relation.</param>
        /// <param name="relationType">The <see cref="RelationTypeIdentifier"/> describing the relation between the first two objects.</param>
        /// <returns>A new <see cref="RelationIdentifier"/>.</returns>
        [Obsolete]
        public static RelationIdentifier BuildRelationIdentifier(ObjectIdentifier subject, ObjectIdentifier obj, RelationTypeIdentifier relationType)
        {
            var relation = new RelationIdentifier();
            relation.Subject = subject;
            relation.Object = obj;
            relation.Relation = relationType;

            return relation;
        }

        /// <summary>
        /// Creates a <see cref="PaginationRequest"/> use for the directory calls.
        /// </summary>
        /// <param name="size">The number of items per page.</param>
        /// <param name="token">The token representing the page from which to start reading.</param>
        /// <returns>A new <see cref="PaginationRequest"/>.</returns>
        [Obsolete]
        public static PaginationRequest BuildPaginationRequest(int size, string token)
        {
            var page = new PaginationRequest();
            page.Size = size;
            page.Token = token;

            return page;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<GetObjectResponse> GetObjectAsync(string key, string type)
        {
            var objIdentifier = BuildObjectIdentifier(key, type);
            var req = new GetObjectRequest();
            req.Param = objIdentifier;
            var result = await ReaderClient().GetObjectAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<GetObjectsResponse> GetObjectsAsync(string type, int pageSize, string pageToken = "")
        {
            var req = new GetObjectsRequest();
            var page = BuildPaginationRequest(pageSize, pageToken);
            var objTypeIdentifier = new ObjectTypeIdentifier();
            objTypeIdentifier.Name = type;
            req.Param = objTypeIdentifier;
            req.Page = page;
            var result = await ReaderClient().GetObjectsAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<GetRelationResponse> GetRelationAsync(string subjectKey = "", string subjectType = "", string objKey = "", string objType = "", string relationName = "", string relationObjectType = "", bool withObjects = false)
        {
            var subjectIdentifier = BuildObjectIdentifier(subjectKey, subjectType);
            var objIdentifier = BuildObjectIdentifier(objKey, objType);
            var relationTypeIdentifier = BuildRelationTypeIdentifier(relationName, relationObjectType);
            var relation = BuildRelationIdentifier(subjectIdentifier, objIdentifier, relationTypeIdentifier);
            var req = new GetRelationRequest();
            req.Param = relation;
            req.WithObjects = withObjects;
            var result = await ReaderClient().GetRelationAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<GetRelationsResponse> GetRelationsAsync(string subjectKey = "", string subjectType = "", string objKey = "", string objType = "", string relationName = "", string relationObjectType = "", int pageSize = 0, string pageToken = "")
        {
            var subjectIdentifier = BuildObjectIdentifier(subjectKey, subjectType);
            var objIdentifier = BuildObjectIdentifier(objKey, objType);
            var relationIdentifier = BuildRelationTypeIdentifier(relationName, relationObjectType);
            var relation = BuildRelationIdentifier(subjectIdentifier, objIdentifier, relationIdentifier);
            var page = BuildPaginationRequest(pageSize, pageToken);
            var req = new GetRelationsRequest();
            req.Param = relation;
            req.Page = page;
            var result = await ReaderClient().GetRelationsAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<CheckPermissionResponse> CheckPermissionAsync(string subjectKey = "", string subjectType = "", string objKey = "", string objType = "", string permissionName = "", bool trace = false)
        {
            var subject = BuildObjectIdentifier(subjectKey, subjectType);
            var obj = BuildObjectIdentifier(objKey, objType);
            var req = new CheckPermissionRequest();
            req.Subject = subject;
            req.Object = obj;
            var permissionIdentifier = new PermissionIdentifier();
            permissionIdentifier.Name = permissionName;
            req.Permission = permissionIdentifier;
            req.Trace = trace;
            var result = await ReaderClient().CheckPermissionAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<CheckRelationResponse> CheckRelationAsync(string subjectKey = "", string subjectType = "", string objKey = "", string objType = "", string relationName = "", string relationObjectType = "", bool trace = false)
        {
            var subject = BuildObjectIdentifier(subjectKey, subjectType);
            var obj = BuildObjectIdentifier(objKey, objType);
            var relation = BuildRelationTypeIdentifier(relationName, relationObjectType);
            var req = new CheckRelationRequest();
            req.Subject = subject;
            req.Object = obj;
            req.Relation = relation;
            req.Trace = trace;
            var result = await ReaderClient().CheckRelationAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<SetObjectResponse> SetObjectAsync(string key, string type, string displayName = "", Struct properties = null, string hash = "")
        {
            var obj = BuildObject(key, type, displayName, properties, hash);
            var req = new SetObjectRequest();
            req.Object = obj;
            var result = await WriterClient().SetObjectAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<DeleteObjectResponse> DeleteObjectAsync(string key, string type)
        {
            var obj = BuildObjectIdentifier(key, type);
            var req = new DeleteObjectRequest();
            req.Param = obj;
            var result = await WriterClient().DeleteObjectAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<SetRelationResponse> SetRelationAsync(string subjectKey, string subjectType, string objKey, string objType, string relationTypeName, string hash = "")
        {
            var subject = BuildObjectIdentifier(subjectKey, subjectType);
            var obj = BuildObjectIdentifier(objKey, objType);
            var relation = BuildRelation(subject, obj, relationTypeName, hash);
            var req = new SetRelationRequest();
            req.Relation = relation;
            var result = await WriterClient().SetRelationAsync(req);

            return result;
        }

        /// <inheritdoc/>
        [Obsolete]
        public async Task<DeleteRelationResponse> DeleteRelationAsync(string subjectKey = "", string subjectType = "", string objKey = "", string objType = "", string relationName = "", string relationObjectType = "")
        {
            var subjectIdentifier = BuildObjectIdentifier(subjectKey, subjectType);
            var objIdentifier = BuildObjectIdentifier(objKey, objType);
            var relationTypeIdentifier = BuildRelationTypeIdentifier(relationName, relationObjectType);
            var relation = BuildRelationIdentifier(subjectIdentifier, objIdentifier, relationTypeIdentifier);
            var req = new DeleteRelationRequest();
            req.Param = relation;
            var result = await WriterClient().DeleteRelationAsync(req);

            return result;
        }

        private ReaderClient ReaderClient()
        {
            if (readerClient == null)
            {
                throw new ArgumentException("reader service address not specified");
            }

            return readerClient;
        }

        private WriterClient WriterClient()
        {
            if (writerClient == null)
            {
                throw new ArgumentException("writer service address not specified");
            }

            return writerClient;
        }

        private Dictionary<string, GrpcChannel> BuildGrpcChannels(AsertoDirectoryOptions opts, GrpcChannelOptions grpcChannelOptions)
        {
            Dictionary<string, GrpcChannel> channels = new Dictionary<string, GrpcChannel>();
            if (!string.IsNullOrEmpty(opts.DirectoryReaderUrl))
            {
                channels[opts.DirectoryReaderUrl] = GrpcChannel.ForAddress(
                options.DirectoryServiceUrl, grpcChannelOptions);
            }

            if (!string.IsNullOrEmpty(opts.DirectoryWriterUrl))
            {
                channels[opts.DirectoryWriterUrl] = GrpcChannel.ForAddress(
                options.DirectoryWriterUrl, grpcChannelOptions);
            }

            if (channels.Count != 2 && !string.IsNullOrEmpty(opts.DirectoryServiceUrl))
            {
                channels[opts.DirectoryServiceUrl] = GrpcChannel.ForAddress(
                options.DirectoryServiceUrl, grpcChannelOptions);
            }

            return channels;
        }
    }
}
