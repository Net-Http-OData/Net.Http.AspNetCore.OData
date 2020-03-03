// -----------------------------------------------------------------------
// <copyright file="ODataRequestMiddleware.cs" company="Project Contributors">
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Net.Http.OData;

namespace Net.Http.AspNetCore.OData
{
    /// <summary>
    /// The OData request middleware.
    /// </summary>
    public sealed class ODataRequestMiddleware
    {
        private static readonly List<string> s_allowedMediaTypes = new List<string> { "application/json", "text/plain" };

        private static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private readonly RequestDelegate _next;

        /// <summary>
        /// Initialises a new instance of the <see cref="ODataRequestMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next piece of middleware in the pipeline.</param>
        public ODataRequestMiddleware(RequestDelegate next) => _next = next;

        /// <summary>
        /// Invokes the middleware component.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current HTTP call.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            ODataRequestOptions requestOptions = default;

            if (httpContext?.Request.IsODataRequest() == true)
            {
                try
                {
                    requestOptions = new ODataRequestOptions(
                        ODataUtility.ODataServiceRootUri(httpContext.Request.Scheme, httpContext.Request.Host.Value, httpContext.Request.Path.Value),
                        ReadIsolationLevel(httpContext.Request),
                        ReadMetadataLevel(httpContext.Request),
                        ReadODataVersion(httpContext.Request));

                    httpContext.Items.Add(typeof(ODataRequestOptions).FullName, requestOptions);
                }
                catch (ODataException exception)
                {
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = (int)exception.StatusCode;

                    string result = JsonSerializer.Serialize(ODataErrorContent.Create(exception), s_jsonSerializerOptions);
                    await httpContext.Response.WriteAsync(result, Encoding.UTF8).ConfigureAwait(false);

                    return;
                }
            }

            await _next(httpContext).ConfigureAwait(false);

            if (requestOptions != null)
            {
                if (!httpContext.Request.IsODataMetadataRequest())
                {
#pragma warning disable CA1308 // Normalize strings to uppercase
                    // TODO: verify this is the correct way to do this...
                    httpContext.Response.Headers["Content-Type"] += ";odata.metadata=" + requestOptions.MetadataLevel.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
                }

                httpContext.Response.Headers.Add(ODataHeaderNames.ODataVersion, requestOptions.Version.ToString());
            }
        }

        private static string ReadHeaderValue(HttpRequest request, string name)
            => request.Headers.TryGetValue(name, out StringValues values) ? values.FirstOrDefault() : default;

        private static ODataIsolationLevel ReadIsolationLevel(HttpRequest request)
        {
            string headerValue = ReadHeaderValue(request, ODataHeaderNames.ODataIsolation);

            if (headerValue != null)
            {
                if (headerValue == "Snapshot")
                {
                    return ODataIsolationLevel.Snapshot;
                }

                throw ODataException.BadRequest($"If specified, the {ODataHeaderNames.ODataIsolation} must be 'Snapshot'.");
            }

            return ODataIsolationLevel.None;
        }

        private static ODataMetadataLevel ReadMetadataLevel(HttpRequest request)
        {
            foreach (MediaTypeHeaderValue header in new RequestHeaders(request.Headers).Accept)
            {
                if (!s_allowedMediaTypes.Contains(header.MediaType.Value))
                {
                    throw ODataException.UnsupportedMediaType(
                        $"A supported MIME type could not be found that matches the acceptable MIME types for the request. The supported type(s) 'application/json;odata.metadata=none, application/json;odata.metadata=minimal, application/json, text/plain' do not match any of the acceptable MIME types '{header.MediaType}'.");
                }

                foreach (NameValueHeaderValue parameter in header.Parameters)
                {
                    if (parameter.Name == ODataMetadataLevelExtensions.HeaderName)
                    {
                        switch (parameter.Value.Value)
                        {
                            case "none":
                                return ODataMetadataLevel.None;

                            case "minimal":
                                return ODataMetadataLevel.Minimal;

                            case "full":
                                throw ODataException.BadRequest(
                                    $"{ODataMetadataLevelExtensions.HeaderName} 'full' is not supported by this service, please use 'none' or 'minimal'.");

                            default:
                                throw ODataException.BadRequest(
                                    $"If specified, the {ODataMetadataLevelExtensions.HeaderName} value in the Accept header must be 'none', 'minimal' or 'full'.");
                        }
                    }
                }
            }

            return ODataMetadataLevel.Minimal;
        }

        private static ODataVersion ReadODataVersion(HttpRequest request)
        {
            string headerValue = ReadHeaderValue(request, ODataHeaderNames.ODataVersion);

            if (headerValue != null)
            {
                if (ODataVersion.TryParse(headerValue, out ODataVersion odataVersion) && odataVersion >= ODataVersion.MinVersion && odataVersion <= ODataVersion.MaxVersion)
                {
                    return odataVersion;
                }
                else
                {
                    throw ODataException.BadRequest(
                        $"If specified, the {ODataHeaderNames.ODataVersion} header must be a valid OData version supported by this service between version {ODataVersion.MinVersion} and {ODataVersion.MaxVersion}.");
                }
            }

            headerValue = ReadHeaderValue(request, ODataHeaderNames.ODataMaxVersion);

            if (headerValue != null)
            {
                if (ODataVersion.TryParse(headerValue, out ODataVersion odataVersion) && odataVersion >= ODataVersion.MinVersion && odataVersion <= ODataVersion.MaxVersion)
                {
                    return odataVersion;
                }
                else
                {
                    throw ODataException.BadRequest(
                        $"If specified, the {ODataHeaderNames.ODataMaxVersion} header must be a valid OData version supported by this service between version {ODataVersion.MinVersion} and {ODataVersion.MaxVersion}.");
                }
            }

            return ODataVersion.MaxVersion;
        }
    }
}
