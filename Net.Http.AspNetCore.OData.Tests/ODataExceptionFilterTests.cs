using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Net.Http.OData;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests
{
    public class ODataExceptionFilterTests
    {
        [Fact]
        public void OnException_DoesNotSet_Result_If_Exception_IsNot_ODataException()
        {
            HttpRequest request = TestHelper.CreateODataHttpRequest("/OData/Products");

            var exceptionContext = new ExceptionContext(new ActionContext(request.HttpContext, new RouteData(), new ActionDescriptor()), new IFilterMetadata[0])
            {
                HttpContext = request.HttpContext,
                Exception = new ArgumentNullException("Name"),
            };

            var exceptionFilter = new ODataExceptionFilter();
            exceptionFilter.OnException(exceptionContext);

            Assert.Null(exceptionContext.Result);
            Assert.False(exceptionContext.ExceptionHandled);
        }

        [Fact]
        public void OnException_Sets_Result_If_Exception_Is_ODataException()
        {
            HttpRequest request = TestHelper.CreateODataHttpRequest("/OData/Products");

            var exceptionContext = new ExceptionContext(new ActionContext(request.HttpContext, new RouteData(), new ActionDescriptor()), new IFilterMetadata[0])
            {
                HttpContext = request.HttpContext,
                Exception = new ODataException("The type 'NorthwindModel.Product' does not contain a property named 'Foo'", HttpStatusCode.BadRequest),
            };

            var exceptionFilter = new ODataExceptionFilter();
            exceptionFilter.OnException(exceptionContext);

            Assert.NotNull(exceptionContext.Result);
            Assert.True(exceptionContext.ExceptionHandled);
        }
    }
}
