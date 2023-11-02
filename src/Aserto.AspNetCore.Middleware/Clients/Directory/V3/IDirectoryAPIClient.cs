//-----------------------------------------------------------------------
// <copyright file="IDirectoryAPIClient.cs" company="Aserto Inc">
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
    public interface IDirectoryAPIClient
    {
        // Reader methods

        /// <summary>
        /// Gets an object.
        /// </summary>
        /// <param name="id">The id of the object.</param>
        /// <param name="type">The type of the object.</param>
        /// <param name="withRelations">If specified the response contains the object's relations.</param>
        /// <returns>A GetObjectResponse object indicating the requested object if found.</returns>
        Task<GetObjectResponse> GetObjectAsync(string id, string type, bool withRelations = false);

        /// <summary>
        /// Gets objects.
        /// </summary>
        /// <param name="type">The type of the objects you want to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="pageToken">The token representing the page from which to start reading.</param>
        /// <returns>A GetObjectsResponse object indicating the requested objects if found.</returns>
        Task<GetObjectsResponse> GetObjectsAsync(string type, int pageSize, string pageToken = "");

        /// <summary>
        /// Gets a relation between 2 objects.
        /// </summary>
        /// <param name="subjectKey">The key of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectRelation">The relation of the subject.</param>
        /// <param name="objKey">The key of the object.</param>
        /// <param name="objType">The type of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="withObjects">A bool indicating if the response should contain the found object for the relation.</param>
        /// <returns>A GetRelationResponse object indicating the relation.</returns>
        Task<GetRelationResponse> GetRelationAsync(string subjectKey = "", string subjectType = "", string subjectRelation = "", string objKey = "", string objType = "", string relationName = "", bool withObjects = false);

        /// <summary>
        /// Gets the relations.
        /// </summary>
        /// <param name="subjectKey">The key of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectRelation">The relation of the subject.</param>
        /// <param name="objKey">The key of the object.</param>
        /// <param name="objType">The type of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="withObjects">A bool indicating if the response should contain the found object for the relation.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="pageToken">The token representing the page from which to start reading.</param>
        /// <returns>A GetRelationsResponse object indicating the relations.</returns>
        Task<GetRelationsResponse> GetRelationsAsync(string subjectKey = "", string subjectType = "", string subjectRelation = "", string objKey = "", string objType = "", string relationName = "", bool withObjects = false, int pageSize = 0, string pageToken = "");

        /// <summary>
        /// Checks permission.
        /// </summary>
        /// <param name="subjectKey">The key of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="objKey">The key of the object.</param>
        /// <param name="objType">The type of the object.</param>
        /// <param name="permissionName">The name of the permission.</param>
        /// <param name="trace">A bool indicating if the trace is enabled.</param>
        /// <returns>A bool indicating if the permission exists.</returns>
        Task<CheckPermissionResponse> CheckPermissionAsync(string subjectKey = "", string subjectType = "", string objKey = "", string objType = "", string permissionName = "", bool trace = false);

        /// <summary>
        /// Checks a relation.
        /// </summary>
        /// <param name="subjectKey">The key of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="objKey">The key of the object.</param>
        /// <param name="objType">The type of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="trace">A bool indicating if the trace is enabled.</param>
        /// <returns>A bool indicating if the relation exists.</returns>
        Task<CheckRelationResponse> CheckRelationAsync(string subjectKey = "", string subjectType = "", string objKey = "", string objType = "", string relationName = "", bool trace = false);

        /// <summary>
        /// Runs a directory check.
        /// </summary>
        /// <param name="subjectId">The key of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="objId">The key of the object.</param>
        /// <param name="objType">The type of the object.</param>
        /// <param name="trace">A bool indicating if the trace is enabled.</param>
        /// <returns>A bool indicating if the relation exists.</returns>
        public Task<CheckResponse> Check(string subjectId = "", string subjectType = "", string relationName = "", string objId = "", string objType = "", bool trace = false);

        // Writer methods

        /// <summary>
        /// Updates an object if it exists, creates one if it doesn't exist.
        /// </summary>
        /// <param name="key">The key of the object.</param>
        /// <param name="type">The type of the object.</param>
        /// <param name="displayName">The display name of the object.</param>
        /// <param name="properties">A struct representing the properties bag of the object.</param>
        /// <param name="hash">The hash of the object.</param>
        /// <returns>A SetObjectResponse object.</returns>
        Task<SetObjectResponse> SetObjectAsync(string key, string type, string displayName = "", Struct properties = null, string hash = "");

        /// <summary>
        /// Deletes an object.
        /// </summary>
        /// <param name="key">The key of the object.</param>
        /// <param name="type">The type of the object.</param>
        /// <param name="withRelations">Remove the relations for this object also.</param>
        /// <returns>A DeleteObjectResponse object.</returns>
        Task<DeleteObjectResponse> DeleteObjectAsync(string key, string type, bool withRelations = false);

        /// <summary>
        /// Creates a relation if it doesn't exists, updates an existing one.
        /// </summary>
        /// <param name="subjectKey">The key of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectRelation">The relaton of the subject.</param>
        /// <param name="objKey">The key of the object.</param>
        /// <param name="objType">The type of the object.</param>
        /// <param name="relationName">The type name of the relation.</param>
        /// <param name="hash">The hash of the object.</param>
        /// <returns>A SetRelationResponse object.</returns>
        Task<SetRelationResponse> SetRelationAsync(string subjectKey, string subjectType, string subjectRelation, string objKey, string objType, string relationName, string hash = "");

        /// <summary>
        /// Deletes a relation.
        /// </summary>
        /// <param name="subjectKey">The key of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="subjectRelation">The relaton of the subject.</param>
        /// <param name="objKey">The key of the object.</param>
        /// <param name="objType">The type of the object.</param>
        /// <param name="relationName">The name of the relation type.</param>
        /// <param name="relationObjectType">The type of the object indicated by relation.</param>
        /// <returns>A DeleteRelationResponse object.</returns>
        Task<DeleteRelationResponse> DeleteRelationAsync(string subjectKey = "", string subjectType = "", string subjectRelation = "", string objKey = "", string objType = "", string relationName = "", string relationObjectType = "");
    }
}
