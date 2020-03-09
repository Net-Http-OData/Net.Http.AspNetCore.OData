using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Net.Http.OData;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests
{
    public class ODataRequestMiddlewareTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public async Task DoesNotAdd_MetadataLevel_ForMetadataRequest()
        {
            HttpRequest request = TestHelper.CreateHttpRequest("/OData/$metadata");

            HttpContext httpContext = request.HttpContext;

            var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
            await middleware.InvokeAsync(httpContext);

            NameValueHeaderValue metadataParameter =
                new ResponseHeaders(httpContext.Response.Headers).ContentType.Parameters.SingleOrDefault(x => x.Name == ODataMetadataLevelExtensions.HeaderName);

            Assert.Null(metadataParameter);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InvokeAsync_ReturnsODataErrorContent_ForApplicationXml()
        {
            HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");
            request.Headers["Accept"] = "application/xml";

            HttpContext httpContext = request.HttpContext;

            var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
            await middleware.InvokeAsync(httpContext);

            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            string bodyResult = new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEnd();

            ODataErrorContent odataErrorContent = JsonSerializer.Deserialize<ODataErrorContent>(bodyResult, TestHelper.JsonSerializerOptions);

            Assert.Equal("415", odataErrorContent.Error.Code);
            Assert.Equal("A supported MIME type could not be found that matches the acceptable MIME types for the request. The supported type(s) 'application/json;odata.metadata=none, application/json;odata.metadata=minimal, application/json, text/plain' do not match any of the acceptable MIME types 'application/xml'.", odataErrorContent.Error.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InvokeAsync_ReturnsODataErrorContent_ForInvalidIsolationLevel()
        {
            HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");
            request.Headers.Add(ODataRequestHeaderNames.ODataIsolation, "ReadCommitted");

            HttpContext httpContext = request.HttpContext;

            var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
            await middleware.InvokeAsync(httpContext);

            Assert.Equal((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            string bodyResult = new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEnd();

            ODataErrorContent odataErrorContent = JsonSerializer.Deserialize<ODataErrorContent>(bodyResult, TestHelper.JsonSerializerOptions);

            Assert.Equal("400", odataErrorContent.Error.Code);
            Assert.Equal("If specified, the OData-Isolation must be 'Snapshot'.", odataErrorContent.Error.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InvokeAsync_ReturnsODataErrorContent_ForInvalidMaxVersion()
        {
            HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");
            request.Headers.Add(ODataRequestHeaderNames.ODataVersion, "4.0");
            request.Headers.Add(ODataRequestHeaderNames.ODataMaxVersion, "3.0");

            HttpContext httpContext = request.HttpContext;

            var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
            await middleware.InvokeAsync(httpContext);

            Assert.Equal((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            string bodyResult = new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEnd();

            ODataErrorContent odataErrorContent = JsonSerializer.Deserialize<ODataErrorContent>(bodyResult, TestHelper.JsonSerializerOptions);

            Assert.Equal("400", odataErrorContent.Error.Code);
            Assert.Equal("The OData version '3.0' specified in the OData-MaxVersion header indicating the maximum acceptable version of the response must be a valid OData version supported by this service between version '4.0' and '4.0'.", odataErrorContent.Error.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InvokeAsync_ReturnsODataErrorContent_ForInvalidMetadataLevel()
        {
            HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");
            request.Headers["Accept"] = "application/json;odata.metadata=all";

            HttpContext httpContext = request.HttpContext;

            var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
            await middleware.InvokeAsync(httpContext);

            Assert.Equal((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            string bodyResult = new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEnd();

            ODataErrorContent odataErrorContent = JsonSerializer.Deserialize<ODataErrorContent>(bodyResult, TestHelper.JsonSerializerOptions);

            Assert.Equal("400", odataErrorContent.Error.Code);
            Assert.Equal("If specified, the odata.metadata value in the Accept header must be 'none', 'minimal' or 'full'.", odataErrorContent.Error.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InvokeAsync_ReturnsODataErrorContent_ForIsolationSnapshot()
        {
            HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");
            request.Headers.Add(ODataRequestHeaderNames.ODataIsolation, "Snapshot");

            HttpContext httpContext = request.HttpContext;

            var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
            await middleware.InvokeAsync(httpContext);

            Assert.Equal((int)HttpStatusCode.PreconditionFailed, httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            string bodyResult = new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEnd();

            ODataErrorContent odataErrorContent = JsonSerializer.Deserialize<ODataErrorContent>(bodyResult, TestHelper.JsonSerializerOptions);

            Assert.Equal("412", odataErrorContent.Error.Code);
            Assert.Equal($"{ODataRequestHeaderNames.ODataIsolation} 'Snapshot' is not supported by this service.", odataErrorContent.Error.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task InvokeAsync_ReturnsODataErrorContent_ForMetadataLevelFull()
        {
            HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");
            request.Headers["Accept"] = "application/json;odata.metadata=full";

            HttpContext httpContext = request.HttpContext;

            var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
            await middleware.InvokeAsync(httpContext);

            Assert.Equal((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);

            httpContext.Response.Body.Position = 0;

            string bodyResult = new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEnd();

            ODataErrorContent odataErrorContent = JsonSerializer.Deserialize<ODataErrorContent>(bodyResult, TestHelper.JsonSerializerOptions);

            Assert.Equal("400", odataErrorContent.Error.Code);
            Assert.Equal("odata.metadata 'full' is not supported by this service, the metadata levels supported by this service are 'none, minimal'.", odataErrorContent.Error.Message);
        }

        public class WhenCalling_InvokeAsync_AndTheResponseHasNoContent
        {
            private readonly HttpResponse _response;

            public WhenCalling_InvokeAsync_AndTheResponseHasNoContent()
            {
                HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");

                HttpContext httpContext = request.HttpContext;

                var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
                middleware.InvokeAsync(httpContext).Wait();

                _response = httpContext.Response;
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void TheMetadataLevelContentTypeParameterIsSetInTheResponse()
            {
                NameValueHeaderValue metadataParameter =
                    new ResponseHeaders(_response.Headers).ContentType.Parameters.SingleOrDefault(x => x.Name == ODataMetadataLevelExtensions.HeaderName);

                Assert.NotNull(metadataParameter);
                Assert.Equal("minimal", metadataParameter.Value);
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void TheODataVersionHeaderIsSetInTheResponse()
            {
                Assert.True(_response.Headers.TryGetValue(ODataResponseHeaderNames.ODataVersion, out StringValues value));
                Assert.Equal(ODataVersion.MaxVersion.ToString(), value);
            }
        }

        public class WhenCalling_InvokeAsync_WithAnODataUri_AndAllRequestOptionsInRequest
        {
            private readonly ODataRequestOptions _odataRequestOptions;
            private readonly HttpResponse _response;

            public WhenCalling_InvokeAsync_WithAnODataUri_AndAllRequestOptionsInRequest()
            {
                HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");
                request.Headers["Accept"] = "application/json;odata.metadata=none";
                request.Headers.Add(ODataRequestHeaderNames.ODataIsolation, "Snapshot");
                request.Headers.Add(ODataRequestHeaderNames.ODataMaxVersion, "4.0");
                request.Headers.Add(ODataRequestHeaderNames.ODataVersion, "4.0");

                var odataServiceOptions = new ODataServiceOptions(
                    ODataVersion.MinVersion,
                    ODataVersion.MaxVersion,
                    new[] { ODataIsolationLevel.Snapshot },
                    new[] { "application/json", "text/plain" });

                HttpContext httpContext = request.HttpContext;

                var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, odataServiceOptions);
                middleware.InvokeAsync(httpContext).Wait();

                _response = httpContext.Response;

                _response.HttpContext.Items.TryGetValue(typeof(ODataRequestOptions).FullName, out object odataRequestOptions);
                _odataRequestOptions = odataRequestOptions as ODataRequestOptions;
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_DataServiceRoot_IsSet()
                => Assert.Equal("https://services.odata.org/OData/", _odataRequestOptions.ServiceRootUri.ToString());

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_IsolationLevel_IsSetTo_Snapshot()
                => Assert.Equal(ODataIsolationLevel.Snapshot, _odataRequestOptions.IsolationLevel);

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_MaxVersion_IsSetTo_4_0()
                => Assert.Equal(ODataVersion.OData40, _odataRequestOptions.ODataMaxVersion);

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_MetadataLevel_IsSetTo_None()
                => Assert.Equal(ODataMetadataLevel.None, _odataRequestOptions.MetadataLevel);

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_Version_IsSetTo_4_0()
                => Assert.Equal(ODataVersion.OData40, _odataRequestOptions.ODataVersion);

            [Fact]
            [Trait("Category", "Unit")]
            public void RequestParameters_ContainsODataRequestOptions()
                => Assert.NotNull(_odataRequestOptions);

            [Fact]
            [Trait("Category", "Unit")]
            public void TheMetadataLevelContentTypeParameterIsSetInTheResponse()
            {
                NameValueHeaderValue metadataParameter =
                    new ResponseHeaders(_response.Headers).ContentType.Parameters.SingleOrDefault(x => x.Name == ODataMetadataLevelExtensions.HeaderName);

                Assert.NotNull(metadataParameter);
                Assert.Equal("none", metadataParameter.Value);
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void TheODataVersionHeaderIsSetInTheResponse()
            {
                Assert.True(_response.Headers.TryGetValue(ODataResponseHeaderNames.ODataVersion, out StringValues headerValue));
                Assert.Equal(ODataVersion.OData40.ToString(), headerValue);
            }
        }

        public class WhenCalling_InvokeAsync_WithAnODataUri_AndNoRequestOptionsInRequest
        {
            private readonly ODataRequestOptions _odataRequestOptions;
            private readonly HttpResponse _response;

            public WhenCalling_InvokeAsync_WithAnODataUri_AndNoRequestOptionsInRequest()
            {
                HttpRequest request = TestHelper.CreateHttpRequest("/OData/Products");

                HttpContext httpContext = request.HttpContext;

                var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
                middleware.InvokeAsync(httpContext).Wait();

                _response = httpContext.Response;

                _response.HttpContext.Items.TryGetValue(typeof(ODataRequestOptions).FullName, out object odataRequestOptions);
                _odataRequestOptions = odataRequestOptions as ODataRequestOptions;
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_DataServiceRoot_IsSet()
                => Assert.Equal("https://services.odata.org/OData/", _odataRequestOptions.ServiceRootUri.ToString());

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_IsolationLevel_DefaultsTo_None()
                => Assert.Equal(ODataIsolationLevel.None, _odataRequestOptions.IsolationLevel);

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_MaxVersion_DefaultsTo_MaxVersion()
                => Assert.Equal(ODataVersion.MaxVersion, _odataRequestOptions.ODataMaxVersion);

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_MetadataLevel_DefaultsTo_Minimal()
                => Assert.Equal(ODataMetadataLevel.Minimal, _odataRequestOptions.MetadataLevel);

            [Fact]
            [Trait("Category", "Unit")]
            public void ODataRequestOptions_Version_DefaultsTo_MaxVersion()
                => Assert.Equal(ODataVersion.MaxVersion, _odataRequestOptions.ODataVersion);

            [Fact]
            [Trait("Category", "Unit")]
            public void RequestParameters_ContainsODataRequestOptions()
                => Assert.NotNull(_odataRequestOptions);

            [Fact]
            [Trait("Category", "Unit")]
            public void TheMetadataLevelContentTypeParameterIsSetInTheResponse()
            {
                NameValueHeaderValue metadataParameter =
                    new ResponseHeaders(_response.Headers).ContentType.Parameters.SingleOrDefault(x => x.Name == ODataMetadataLevelExtensions.HeaderName);

                Assert.NotNull(metadataParameter);
                Assert.Equal("minimal", metadataParameter.Value);
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void TheODataVersionHeaderIsSetInTheResponse()
            {
                Assert.True(_response.Headers.TryGetValue(ODataResponseHeaderNames.ODataVersion, out StringValues headerValue));
                Assert.Equal(ODataVersion.MaxVersion.ToString(), headerValue);
            }
        }

        public class WhenCalling_InvokeAsync_WithoutAnODataUri
        {
            private readonly HttpResponse _response;

            public WhenCalling_InvokeAsync_WithoutAnODataUri()
            {
                HttpRequest request = TestHelper.CreateHttpRequest("/api/Products");

                HttpContext httpContext = request.HttpContext;

                var middleware = new ODataRequestMiddleware((HttpContext context) => Task.CompletedTask, TestHelper.ODataServiceOptions);
                middleware.InvokeAsync(httpContext).Wait();

                _response = httpContext.Response;
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void RequestParameters_DoesNotContainsODataRequestOptions()
            {
                Assert.False(_response.HttpContext.Items.TryGetValue(typeof(ODataRequestOptions).FullName, out object odataRequestOptions));
                Assert.Null(odataRequestOptions);
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void TheMetadataLevelContentTypeParameterIsNotSet()
                => Assert.DoesNotContain(new ResponseHeaders(_response.Headers).ContentType.Parameters, x => x.Name == ODataMetadataLevelExtensions.HeaderName);

            [Fact]
            [Trait("Category", "Unit")]
            public void TheODataVersionHeaderIsNotSet()
                => Assert.False(_response.Headers.ContainsKey(ODataResponseHeaderNames.ODataVersion));
        }
    }
}
