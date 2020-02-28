using Microsoft.AspNetCore.Mvc;
using Net.Http.AspNetCore.OData.Metadata;
using Net.Http.OData;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests.Metadata
{
    public class ServiceDocumentODataControllerTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void WhenFullMetadataIsRequested_TheEntitySetUrlIsRelative_AndTheContextUriIsSet()
        {
            TestHelper.EnsureEDM();

            var controller = new ServiceDocumentODataController
            {
                ControllerContext = new ControllerContext { HttpContext = TestHelper.CreateODataHttpRequest("/OData", ODataMetadataLevel.Full).HttpContext },
            };

            IActionResult result = controller.Get();

            Assert.IsType<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            object value = okResult.Value;

            Assert.IsType<ODataResponseContent>(value);

            var odataResponseContent = (ODataResponseContent)value;

            Assert.Equal("https://services.odata.org/OData/$metadata", odataResponseContent.Context);
            Assert.Null(odataResponseContent.Count);
            Assert.Null(odataResponseContent.NextLink);
            Assert.NotNull(odataResponseContent.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void WhenMinimalMetadataIsRequested_TheEntitySetUrlIsRelative_AndTheContextUriIsSet()
        {
            TestHelper.EnsureEDM();

            var controller = new ServiceDocumentODataController
            {
                ControllerContext = new ControllerContext { HttpContext = TestHelper.CreateODataHttpRequest("/OData", ODataMetadataLevel.Minimal).HttpContext },
            };

            IActionResult result = controller.Get();

            Assert.IsType<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            object value = okResult.Value;

            Assert.IsType<ODataResponseContent>(value);

            var odataResponseContent = (ODataResponseContent)value;

            Assert.Equal("https://services.odata.org/OData/$metadata", odataResponseContent.Context);
            Assert.Null(odataResponseContent.Count);
            Assert.Null(odataResponseContent.NextLink);
            Assert.NotNull(odataResponseContent.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void WhenNoMetadataIsRequested_TheEntitySetUrlIsFullUrl_AndTheContextUriIsNotSet()
        {
            TestHelper.EnsureEDM();

            var controller = new ServiceDocumentODataController
            {
                ControllerContext = new ControllerContext { HttpContext = TestHelper.CreateODataHttpRequest("/OData", ODataMetadataLevel.None).HttpContext },
            };

            IActionResult result = controller.Get();

            Assert.IsType<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;

            object value = okResult.Value;

            Assert.IsType<ODataResponseContent>(value);

            var odataResponseContent = (ODataResponseContent)value;

            Assert.Null(odataResponseContent.Context);
            Assert.Null(odataResponseContent.Count);
            Assert.Null(odataResponseContent.NextLink);
            Assert.NotNull(odataResponseContent.Value);
        }
    }
}
