using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Net.Http.AspNetCore.OData.Metadata;
using Net.Http.OData;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests.Metadata
{
    public class ServiceDocumentODataControllerTests
    {
        private static readonly System.Text.Json.JsonSerializerOptions s_jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };

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

            string serviceDocument = JsonSerializer.Serialize(odataResponseContent, s_jsonSerializerOptions);

            Assert.Equal(
                "{\"@odata.context\":\"https://services.odata.org/OData/$metadata\",\"value\":[{\"kind\":\"EntitySet\",\"name\":\"Categories\",\"url\":\"Categories\"},{\"kind\":\"EntitySet\",\"name\":\"Customers\",\"url\":\"Customers\"},{\"kind\":\"EntitySet\",\"name\":\"Employees\",\"url\":\"Employees\"},{\"kind\":\"EntitySet\",\"name\":\"Managers\",\"url\":\"Managers\"},{\"kind\":\"EntitySet\",\"name\":\"Orders\",\"url\":\"Orders\"},{\"kind\":\"EntitySet\",\"name\":\"Products\",\"url\":\"Products\"}]}",
                serviceDocument);
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

            string serviceDocument = JsonSerializer.Serialize(odataResponseContent, s_jsonSerializerOptions);

            Assert.Equal(
                "{\"@odata.context\":\"https://services.odata.org/OData/$metadata\",\"value\":[{\"kind\":\"EntitySet\",\"name\":\"Categories\",\"url\":\"Categories\"},{\"kind\":\"EntitySet\",\"name\":\"Customers\",\"url\":\"Customers\"},{\"kind\":\"EntitySet\",\"name\":\"Employees\",\"url\":\"Employees\"},{\"kind\":\"EntitySet\",\"name\":\"Managers\",\"url\":\"Managers\"},{\"kind\":\"EntitySet\",\"name\":\"Orders\",\"url\":\"Orders\"},{\"kind\":\"EntitySet\",\"name\":\"Products\",\"url\":\"Products\"}]}",
                serviceDocument);
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

            string serviceDocument = JsonSerializer.Serialize(odataResponseContent, s_jsonSerializerOptions);

            Assert.Equal(
                "{\"value\":[{\"kind\":\"EntitySet\",\"name\":\"Categories\",\"url\":\"https://services.odata.org/OData/Categories\"},{\"kind\":\"EntitySet\",\"name\":\"Customers\",\"url\":\"https://services.odata.org/OData/Customers\"},{\"kind\":\"EntitySet\",\"name\":\"Employees\",\"url\":\"https://services.odata.org/OData/Employees\"},{\"kind\":\"EntitySet\",\"name\":\"Managers\",\"url\":\"https://services.odata.org/OData/Managers\"},{\"kind\":\"EntitySet\",\"name\":\"Orders\",\"url\":\"https://services.odata.org/OData/Orders\"},{\"kind\":\"EntitySet\",\"name\":\"Products\",\"url\":\"https://services.odata.org/OData/Products\"}]}",
                serviceDocument);
        }
    }
}
