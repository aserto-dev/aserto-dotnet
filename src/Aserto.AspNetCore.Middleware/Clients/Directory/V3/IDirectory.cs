//-----------------------------------------------------------------------
// <copyright file="IDirectory.cs" company="Aserto Inc">
// Copyright (c) Aserto Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Aserto.AspNetCore.Middleware.Clients.Directory.V3
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Aserto.Directory.Common.V3;
    using Aserto.Directory.Exporter.V3;
    using Aserto.Directory.Importer.V3;
    using Aserto.Directory.Model.V3;
    using Aserto.Directory.Reader.V3;
    using Aserto.Directory.Writer.V3;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Client for communicating with the Aserto Directory API.
    /// </summary>
    public interface IDirectory
    {
        // Reader methods

        /// <summary>
        /// Gets an object.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="id">The id of the object.</param>
        /// <param name="withRelations">If specified the response contains the object's relations.</param>
        /// <returns>A GetObjectResponse object indicating the requested object if found.</returns>
        Task<GetObjectResponse> GetObjectAsync(string type, string id, bool withRelations = false);

        /// <summary>
        /// Gets objects.
        /// </summary>
        /// <param name="type">The type of the objects you want to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="pageToken">The token representing the page from which to start reading.</param>
        /// <returns>A GetObjectsResponse object indicating the requested objects if found.</returns>
        Task<GetObjectsResponse> GetObjectsAsync(string type, int pageSize = 50, string pageToken = "");

        /// <summary>
        /// Gets a relation between 2 objects.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The Id of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The Id of the subject.</param>
        /// <param name="subjectRelation">The relation of the subject.</param>
        /// <param name="withObjects">A bool indicating if the response should contain the found object for the relation.</param>
        /// <returns>A GetRelationResponse object indicating the relation.</returns>
        Task<GetRelationResponse> GetRelationAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "",  string subjectRelation = "",  bool withObjects = false);

        /// <summary>
        /// Gets the relations.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The Id of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The Id of the subject.</param>
        /// <param name="subjectRelation">The relation of the subject.</param>
        /// <param name="withObjects">A bool indicating if the response should contain the found object for the relation.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="pageToken">The token representing the page from which to start reading.</param>
        /// <returns>A GetRelationsResponse object indicating the relations.</returns>
        Task<GetRelationsResponse> GetRelationsAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", string subjectRelation = "", bool withObjects = false, int pageSize = 50, string pageToken = "");

        /// <summary>
        /// Checks permission.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The Id of the object.</param>
        /// <param name="permissionName">The name of the permission.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The Id of the subject.</param>
        /// <param name="trace">A bool indicating if the trace is enabled.</param>
        /// <returns>A bool indicating if the permission exists.</returns>
        Task<CheckPermissionResponse> CheckPermissionAsync(string objType = "", string objId = "", string permissionName = "", string subjectType = "", string subjectId = "",   bool trace = false);

        /// <summary>
        /// Checks a relation.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The Id of the object.</param>
        /// <param name="relationName">The name of the relation.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The Id of the subject.</param>
        /// <param name="trace">A bool indicating if the trace is enabled.</param>
        /// <returns>A bool indicating if the relation exists.</returns>
        Task<CheckRelationResponse> CheckRelationAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", bool trace = false);

        /// <summary>
        /// Runs a directory check.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The Id of the object.</param>
        /// <param name="relationName">The name of the relation.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The Id of the subject.</param>
        /// <param name="trace">A bool indicating if the trace is enabled.</param>
        /// <returns>A bool indicating if the relation exists.</returns>
        public Task<CheckResponse> CheckAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", bool trace = false);

        // Writer methods

        /// <summary>
        /// Updates an object if it exists, creates one if it doesn't exist.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="id">The id of the object.</param>
        /// <param name="displayName">The display name of the object.</param>
        /// <param name="properties">A struct representing the properties bag of the object.</param>
        /// <param name="hash">The hash of the object.</param>
        /// <returns>A SetObjectResponse object.</returns>
        Task<SetObjectResponse> SetObjectAsync(string type, string id, string displayName = "", Struct properties = null, string hash = "");

        /// <summary>
        /// Deletes an object.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="id">The id of the object.</param>
        /// <param name="withRelations">Remove the relations for this object also.</param>
        /// <returns>A DeleteObjectResponse object.</returns>
        Task<DeleteObjectResponse> DeleteObjectAsync(string type, string id, bool withRelations = false);

        /// <summary>
        /// Creates a relation if it doesn't exists, updates an existing one.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The id of the object.</param>
        /// <param name="relationName">The type name of the relation.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The id of the subject.</param>
        /// <param name="subjectRelation">The relaton of the subject.</param>
        /// <param name="hash">The hash of the object.</param>
        /// <returns>A SetRelationResponse object.</returns>
        Task<SetRelationResponse> SetRelationAsync(string objType, string objId, string relationName, string subjectType, string subjectId,  string subjectRelation, string hash = "");

        /// <summary>
        /// Deletes a relation.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The Id of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The Id of the subject.</param>
        /// <param name="subjectRelation">The relaton of the subject.</param>
        /// <returns>A DeleteRelationResponse object.</returns>
        Task<DeleteRelationResponse> DeleteRelationAsync(string objType = "", string objId = "", string relationName = "", string subjectType = "", string subjectId = "", string subjectRelation = "");

        /// <summary>
        /// Get graph of directory objects.
        /// </summary>
        /// <param name="objType">The type of the object.</param>
        /// <param name="objId">The Id of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectId">The Id of the subject.</param>
        /// <param name="subjectRelation">The relaton of the subject.</param>
        /// <param name="explain">Explain the graph ressult.</param>
        /// <param name="trace">Trace the graph result.</param>
        /// <returns>A GetGraphResponse object.</returns>
        Task<GetGraphResponse> GetGraphAsync(string objType, string objId, string relationName, string subjectType, string subjectId, string subjectRelation, bool explain = false, bool trace = false);

        /// <summary>
        /// Get the directory manifest.
        /// </summary>
        /// <param name="request">Get manifest request.</param>
        /// <returns>A GetManifestResponse object.</returns>
        Task<GetManifestResponse> GetManifestAsync(GetManifestRequest request);

        /// <summary>
        /// Set the directory manifest.
        /// </summary>
        /// <param name="request">Set Manifest request.</param>
        /// <returns>A SetManifestResponse object.</returns>
        Task<SetManifestResponse> SetManifestAsync(SetManifestRequest request);

        /// <summary>
        /// Delete the directory manifest.
        /// </summary>
        /// <param name="request">A DeleteManifest request object.</param>
        /// <returns>A DeleteManifestResponse object.</returns>
        Task<DeleteManifestResponse> DeleteManifestAsync(DeleteManifestRequest request);

        /// <summary>
        /// Import data into the directory.
        /// </summary>
        /// <param name="request">An ImportRequest object.</param>
        /// <returns>An ImportResponse object.</returns>
        IAsyncEnumerable<ImportResponse> ImportAsync(ImportRequest request);

        /// <summary>
        /// Export data from the directory.
        /// </summary>
        /// <param name="request">An ExportRequest object.</param>
        /// <returns>An ExportResponse object.</returns>
        IAsyncEnumerable<ExportResponse> ExportAsync(ExportRequest request);
    }
}
