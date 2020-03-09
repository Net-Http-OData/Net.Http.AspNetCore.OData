using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Net.Http.OData.Query;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests
{
    public class ODataQueryOptionsModelBinderTests
    {
        [Fact]
        public void BindModelAsync_SetsODataQueryOptions()
        {
            TestHelper.EnsureEDM();

            HttpRequest request = TestHelper.CreateODataHttpRequest("/OData/Products?$count=true");

            var mockModelBindingContext = new Mock<ModelBindingContext>();
            mockModelBindingContext.Setup(x => x.HttpContext).Returns(request.HttpContext);
            mockModelBindingContext.SetupProperty(x => x.Result);

            ModelBindingContext modelBindingContext = mockModelBindingContext.Object;

            var modelBinder = new ODataQueryOptionsModelBinder();
            modelBinder.BindModelAsync(modelBindingContext).Wait();

            Assert.True(modelBindingContext.Result.IsModelSet);
            Assert.IsType<ODataQueryOptions>(modelBindingContext.Result.Model);

            var queryOptions = (ODataQueryOptions)modelBindingContext.Result.Model;
            Assert.Equal("Products", queryOptions.EntitySet.Name);
            Assert.Equal("?$count=true", queryOptions.RawValues.ToString());
        }
    }
}
