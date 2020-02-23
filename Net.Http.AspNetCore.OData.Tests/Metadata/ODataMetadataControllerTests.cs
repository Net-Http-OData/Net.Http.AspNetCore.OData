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

            IActionResult result = controller.Get();

            Assert.IsType<ContentResult>(result);

            var contentResult = (ContentResult)result;

            string content = contentResult.Content;

            Assert.NotNull(content);

            var resultXml = XDocument.Parse(content);

            Assert.Equal("Edmx", resultXml.Root.Name.LocalName);
        }
    }
}
