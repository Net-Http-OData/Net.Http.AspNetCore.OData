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
        private static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private readonly RequestDelegate _next;
        private readonly ODataServiceOptions _odataServiceOptions;

        /// <summary>
        /// Initialises a new instance of the <see cref="ODataRequestMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next piece of middleware in the pipeline.</param>
        /// <param name="odataServiceOptions">The <see cref="ODataServiceOptions"/> for the service.</param>
        public ODataRequestMiddleware(RequestDelegate next, ODataServiceOptions odataServiceOptions)
        {
            _next = next;
            _odataServiceOptions = odataServiceOptions;
        }

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
                    if (!httpContext.Request.IsODataMetadataRequest())
                    {
                        _odataServiceOptions.Validate(new RequestHeaders(httpContext.Request.Headers).Accept.Select(x => x.MediaType.Value));
                    }

                    requestOptions = new ODataRequestOptions(
                        ODataUtility.ODataServiceRootUri(httpContext.Request.Scheme, httpContext.Request.Host.Value, httpContext.Request.Path.Value),
                        ReadIsolationLevel(httpContext.Request),
                        ReadMetadataLevel(httpContext.Request),
                        ReadODataVersion(httpContext.Request),
                        ReadODataMaxVersion(httpContext.Request));

                    _odataServiceOptions.Validate(requestOptions);

                    httpContext.Items.Add(typeof(ODataRequestOptions).FullName, requestOptions);
                }
                catch (ODataException exception)
                {
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = (int)exception.StatusCode;

                    string result = JsonSerializer.Serialize(exception.ToODataErrorContent(), s_jsonSerializerOptions);
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

                httpContext.Response.Headers.Add(ODataResponseHeaderNames.ODataVersion, requestOptions.ODataMaxVersion.ToString());
            }
        }

        private static string ReadHeaderValue(HttpRequest request, string name)
            => request.Headers.TryGetValue(name, out StringValues values) ? values.FirstOrDefault() : default;

        private static ODataIsolationLevel ReadIsolationLevel(HttpRequest request)
        {
            string headerValue = ReadHeaderValue(request, ODataRequestHeaderNames.ODataIsolation);

            if (headerValue != null)
            {
                if (headerValue == "Snapshot")
                {
                    return ODataIsolationLevel.Snapshot;
                }

                throw ODataException.BadRequest($"If specified, the {ODataRequestHeaderNames.ODataIsolation} must be 'Snapshot'.");
            }

            return ODataIsolationLevel.None;
        }

        private static ODataMetadataLevel ReadMetadataLevel(HttpRequest request)
        {
            foreach (MediaTypeHeaderValue header in new RequestHeaders(request.Headers).Accept)
            {
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
                                return ODataMetadataLevel.Full;

                            default:
                                throw ODataException.BadRequest(
                                    $"If specified, the {ODataMetadataLevelExtensions.HeaderName} value in the Accept header must be 'none', 'minimal' or 'full'.");
                        }
                    }
                }
            }

            return ODataMetadataLevel.Minimal;
        }

        private ODataVersion ReadODataMaxVersion(HttpRequest request)
        {
            string headerValue = ReadHeaderValue(request, ODataRequestHeaderNames.ODataMaxVersion);

            if (headerValue != null)
            {
                if (ODataVersion.TryParse(headerValue, out ODataVersion odataVersion))
                {
                    return odataVersion;
                }

                throw ODataException.BadRequest(
                    $"If specified, the {ODataRequestHeaderNames.ODataMaxVersion} header must be a valid OData version supported by this service between version {_odataServiceOptions.MinVersion} and {_odataServiceOptions.MaxVersion}.");
            }

            return ODataVersion.MaxVersion;
        }

        private ODataVersion ReadODataVersion(HttpRequest request)
        {
            string headerValue = ReadHeaderValue(request, ODataRequestHeaderNames.ODataVersion);

            if (headerValue != null)
            {
                if (ODataVersion.TryParse(headerValue, out ODataVersion odataVersion))
                {
                    return odataVersion;
                }

                throw ODataException.BadRequest(
                    $"If specified, the {ODataRequestHeaderNames.ODataVersion} header must be a valid OData version supported by this service between version {_odataServiceOptions.MinVersion} and {_odataServiceOptions.MaxVersion}.");
            }

            return ReadODataMaxVersion(request);
        }
    }
}
