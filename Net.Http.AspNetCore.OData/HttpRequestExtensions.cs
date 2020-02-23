// -----------------------------------------------------------------------
// <copyright file="HttpRequestExtensions.cs" company="Project Contributors">
// Copyright Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.Http.OData;
using Net.Http.OData.Model;
using Net.Http.OData.Query;

namespace Net.Http.AspNetCore.OData
{
    /// <summary>
    /// Extensions for the <see cref="HttpRequest"/> class.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the request is an OData Metadata request.
        /// </summary>
        /// <param name="request">The HTTP request for the current request.</param>
        /// <returns>True if the request is an OData Metadata request, otherwise false.</returns>
        public static bool IsODataMetadataRequest(this HttpRequest request)
            => request?.Path.Value.IndexOf("$metadata", StringComparison.OrdinalIgnoreCase) > 0;

        /// <summary>
        /// Gets a value indicating whether the request is an OData request.
        /// </summary>
        /// <param name="request">The HTTP request for the current request.</param>
        /// <returns>True if the request is an OData request, otherwise false.</returns>
        public static bool IsODataRequest(this HttpRequest request)
            => request?.Path.Value.IndexOf("odata", StringComparison.OrdinalIgnoreCase) > 0;

        /// <summary>
        /// Gets the @odata.nextLink for a paged OData query.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <param name="queryOptions">The query options.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="resultsPerPage">The results per page.</param>
        /// <returns>The next link for a paged OData query.</returns>
        public static string NextLink(this HttpRequest request, ODataQueryOptions queryOptions, int skip, int resultsPerPage)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (queryOptions is null)
            {
                throw new ArgumentNullException(nameof(queryOptions));
            }

            StringBuilder uriBuilder = new StringBuilder()
                .Append(request.Scheme)
                .Append(Uri.SchemeDelimiter)
                .Append(request.Host)
                .Append(request.Path.Value)
                .Append("?$skip=").Append((skip + resultsPerPage).ToString(CultureInfo.InvariantCulture));

            if (queryOptions.RawValues.Count != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.Count);
            }

            if (queryOptions.RawValues.Expand != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.Expand);
            }

            if (queryOptions.RawValues.Filter != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.Filter);
            }

            if (queryOptions.RawValues.Format != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.Format);
            }

            if (queryOptions.RawValues.OrderBy != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.OrderBy);
            }

            if (queryOptions.RawValues.Search != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.Search);
            }

            if (queryOptions.RawValues.Select != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.Select);
            }

            if (queryOptions.RawValues.Top != null)
            {
                uriBuilder.Append('&').Append(queryOptions.RawValues.Top);
            }

            return uriBuilder.ToString();
        }

        /// <summary>
        /// Reads the OData request options.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <returns>The OData request options for the request.</returns>
        public static ODataRequestOptions ReadODataRequestOptions(this HttpRequest request)
            => request?.HttpContext.Items[typeof(ODataRequestOptions).FullName] as ODataRequestOptions;

        /// <summary>
        /// Resolves the <see cref="EntitySet"/> for the OData request.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <returns>The EntitySet the OData request relates to.</returns>
        public static EntitySet ResolveEntitySet(this HttpRequest request)
            => EntityDataModel.Current.EntitySetForPath(request?.HttpContext.Request.Path.Value);

        /// <summary>
        /// Resolves the @odata.context for the specified request.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <returns>A <see cref="string"/> containing the @odata.context, or null if the metadata for the request is none.</returns>
        public static string ResolveODataContext(this HttpRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataRequestOptions requestOptions = request.ReadODataRequestOptions();

            return ODataUtility.ODataContext(
                requestOptions.MetadataLevel,
                request.Scheme,
                request.Host.Value,
                request.Path);
        }

        /// <summary>
        /// Resolves the @odata.context for the specified request and Entity Set.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <param name="entitySet">The EntitySet used in the request.</param>
        /// <returns>A <see cref="string"/> containing the @odata.context, or null if the metadata for the request is none.</returns>
        public static string ResolveODataContext(this HttpRequest request, EntitySet entitySet)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataRequestOptions requestOptions = request.ReadODataRequestOptions();

            return ODataUtility.ODataContext(
                requestOptions.MetadataLevel,
                request.Scheme,
                request.Host.Value,
                request.Path,
                entitySet);
        }

        /// <summary>
        /// Resolves the @odata.context for the specified request and Entity Set and select query option.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <param name="entitySet">The EntitySet used in the request.</param>
        /// <param name="selectQueryOption">The select query option.</param>
        /// <returns>A <see cref="string"/> containing the @odata.context URI, or null if the metadata for the request is none.</returns>
        public static string ResolveODataContext(this HttpRequest request, EntitySet entitySet, SelectExpandQueryOption selectQueryOption)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataRequestOptions requestOptions = request.ReadODataRequestOptions();

            return ODataUtility.ODataContext(
                requestOptions.MetadataLevel,
                request.Scheme,
                request.Host.Value,
                request.Path,
                entitySet,
                selectQueryOption);
        }

        /// <summary>
        /// Resolves the @odata.context for the specified request and Entity Set.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <param name="entitySet">The EntitySet used in the request.</param>
        /// <typeparam name="TEntityKey">The type of entity key.</typeparam>
        /// <returns>A <see cref="string"/> containing the @odata.context, or null if the metadata for the request is none.</returns>
        public static string ResolveODataContext<TEntityKey>(this HttpRequest request, EntitySet entitySet)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataRequestOptions requestOptions = request.ReadODataRequestOptions();

            return ODataUtility.ODataContext<TEntityKey>(
                requestOptions.MetadataLevel,
                request.Scheme,
                request.Host.Value,
                request.Path,
                entitySet);
        }

        /// <summary>
        /// Resolves the @odata.context for the specified request and Entity Set.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <param name="entitySet">The EntitySet used in the request.</param>
        /// <param name="entityKey">The Entity Key for the item in the EntitySet.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <typeparam name="TEntityKey">The type of entity key.</typeparam>
        /// <returns>A <see cref="string"/> containing the @odata.context URI, or null if the metadata for the request is none.</returns>
        public static string ResolveODataContext<TEntityKey>(this HttpRequest request, EntitySet entitySet, TEntityKey entityKey, string propertyName)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ODataRequestOptions requestOptions = request.ReadODataRequestOptions();

            return ODataUtility.ODataContext(
                requestOptions.MetadataLevel,
                request.Scheme,
                request.Host.Value,
                request.Path,
                entitySet,
                entityKey,
                propertyName);
        }

        /// <summary>
        /// Resolves the @odata.id for the specified request and Entity Set.
        /// </summary>
        /// <param name="request">The HTTP request which led to this OData request.</param>
        /// <param name="entitySet">The EntitySet used in the request.</param>
        /// <param name="entityKey">The Entity Key for the item in the EntitySet.</param>
        /// <typeparam name="TEntityKey">The type of entity key.</typeparam>
        /// <returns>A <see cref="string"/> containing the address of the Entity with the specified Entity Key.</returns>
        public static string ResolveODataId<TEntityKey>(this HttpRequest request, EntitySet entitySet, TEntityKey entityKey)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return ODataUtility.ODataId(
                request.Scheme,
                request.Host.Value,
                request.Path,
                entitySet,
                entityKey);
        }

        /// <summary>
        /// Creates the OData error response from the specified exception.
        /// </summary>
        /// <param name="request">The HTTP request which led to the error.</param>
        /// <param name="exception">The <see cref="ODataException"/> indicating the error.</param>
        /// <returns>An <see cref="IActionResult"/> representing the OData error.</returns>
        internal static IActionResult CreateODataErrorResult(this HttpRequest request, ODataException exception)
            => new ObjectResult(ODataErrorContent.Create(exception)) { StatusCode = (int)exception?.StatusCode };
    }
}
