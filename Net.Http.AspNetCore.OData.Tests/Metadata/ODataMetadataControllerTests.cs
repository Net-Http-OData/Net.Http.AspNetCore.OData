using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Net.Http.AspNetCore.OData.Metadata;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests.Metadata
{
    public class ODataMetadataControllerTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void GetReturnsCsdlXmlDocument()
        {
            TestHelper.EnsureEDM();

            var controller = new ODataMetadataController();

            IActionResult response = controller.Get();

            Assert.IsType<ContentResult>(response);

            var contentResult = (ContentResult)response;

            string result = contentResult.Content;

            Assert.NotNull(result);

            var resultXml = XDocument.Parse(result);

            Assert.Equal("Edmx", resultXml.Root.Name.LocalName);
        }
    }
}
