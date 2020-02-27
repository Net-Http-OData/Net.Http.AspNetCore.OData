using Microsoft.AspNetCore.Http;
using Moq;
using Net.Http.OData;
using Net.Http.OData.Model;
using Net.Http.OData.Query;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests
{
    public class HttpRequestExtensionsTests
    {
        [Theory]
        [InlineData("/OData")]
        [InlineData("/OData/Products")]
        [Trait("Category", "Unit")]
        public void IsODataMetadataRequest_False(string path)
        {
            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest(path);

            Assert.False(httpRequest.IsODataMetadataRequest());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IsODataMetadataRequest_True()
        {
            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/odata/$metadata");

            Assert.True(httpRequest.IsODataMetadataRequest());
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/api")]
        [Trait("Category", "Unit")]
        public void IsODataRequest_False(string path)
        {
            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest(path);

            Assert.False(httpRequest.IsODataRequest());
        }

        [Theory]
        [InlineData("/OData")]
        [InlineData("/OData/Products")]
        [Trait("Category", "Unit")]
        public void IsODataRequest_True(string path)
        {
            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest(path);

            Assert.True(httpRequest.IsODataRequest());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NextLink_WithAllQueryOptions()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest(
            	"/OData/Products?$count=true&$expand=Category&$filter=Name eq 'Milk'&$format=json&$orderby=Name&$search=blue OR green&$select=Name,Price&$top=25");

            ODataQueryOptions queryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                EntityDataModel.Current.EntitySets["Products"],
                Mock.Of<IODataQueryOptionsValidator>());

            Assert.Equal(
                "https://services.odata.org/OData/Products?$skip=75&$count=true&$expand=Category&$filter=Name eq 'Milk'&$format=json&$orderby=Name&$search=blue OR green&$select=Name,Price&$top=25",
                httpRequest.NextLink(queryOptions, 50, 25));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NextLink_WithTopQueryOption()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products?$top=25");

            ODataQueryOptions queryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                EntityDataModel.Current.EntitySets["Products"],
                Mock.Of<IODataQueryOptionsValidator>());

            Assert.Equal(
                "https://services.odata.org/OData/Products?$skip=25&$top=25",
                httpRequest.NextLink(queryOptions, 0, 25));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_ReturnsContext_IfMetadataIsFull()
        {
            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData", ODataMetadataLevel.Full);

            string odataContext = httpRequest.ResolveODataContext();

            Assert.Equal("https://services.odata.org/OData/$metadata", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_ReturnsContext_IfMetadataIsMinimal()
        {
            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData", ODataMetadataLevel.Minimal);

            string odataContext = httpRequest.ResolveODataContext();

            Assert.Equal("https://services.odata.org/OData/$metadata", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_ReturnsNull_IfMetadataIsNone()
        {
            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData", ODataMetadataLevel.None);

            string odataContext = httpRequest.ResolveODataContext();

            Assert.Null(odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndEntityKey_ReturnsContext_IfMetadataIsFull()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products('Milk')", ODataMetadataLevel.Full);

            string odataContext = httpRequest.ResolveODataContext<string>(EntityDataModel.Current.EntitySets["Products"]);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products/$entity", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndEntityKey_ReturnsContext_IfMetadataIsMinimal()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products('Milk')", ODataMetadataLevel.Minimal);

            string odataContext = httpRequest.ResolveODataContext<string>(EntityDataModel.Current.EntitySets["Products"]);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products/$entity", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndEntityKey_ReturnsNull_IfMetadataIsNone()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products('Milk')", ODataMetadataLevel.None);

            string odataContext = httpRequest.ResolveODataContext<string>(EntityDataModel.Current.EntitySets["Products"]);

            Assert.Null(odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndIntEntityKey_AndProperty_ReturnsContext_IfMetadataIsFull()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Orders(12345)/Name", ODataMetadataLevel.Full);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Orders"], 12345, "Name");

            Assert.Equal("https://services.odata.org/OData/$metadata#Orders(12345)/Name", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndIntEntityKey_AndProperty_ReturnsContext_IfMetadataIsMinimal()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Orders(12345)/Name", ODataMetadataLevel.Minimal);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Orders"], 12345, "Name");

            Assert.Equal("https://services.odata.org/OData/$metadata#Orders(12345)/Name", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndIntEntityKey_AndProperty_ReturnsNull_IfMetadataIsNone()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Orders(12345)/Name", ODataMetadataLevel.None);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Orders"], 12345, "Name");

            Assert.Null(odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndStringEntityKey_AndProperty_ReturnsContext_IfMetadataIsFull()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products('Milk')/Name", ODataMetadataLevel.Full);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Products"], "Milk", "Name");

            Assert.Equal("https://services.odata.org/OData/$metadata#Products('Milk')/Name", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndStringEntityKey_AndProperty_ReturnsContext_IfMetadataIsMinimal()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products('Milk')/Name", ODataMetadataLevel.Minimal);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Products"], "Milk", "Name");

            Assert.Equal("https://services.odata.org/OData/$metadata#Products('Milk')/Name", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_AndStringEntityKey_AndProperty_ReturnsNull_IfMetadataIsNone()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products('Milk')/Name", ODataMetadataLevel.None);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Products"], "Milk", "Name");

            Assert.Null(odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_ReturnsContext_IfMetadataIsFull()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products", ODataMetadataLevel.Full);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Products"]);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_ReturnsContext_IfMetadataIsMinimal()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products", ODataMetadataLevel.Minimal);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Products"]);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySet_ReturnsNull_IfMetadataIsNone()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products", ODataMetadataLevel.None);

            string odataContext = httpRequest.ResolveODataContext(EntityDataModel.Current.EntitySets["Products"]);

            Assert.Null(odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySetAndSelectExpandQueryOptionAll_ReturnsContext_IfMetadataIsFull()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products?$select=*", ODataMetadataLevel.Full);

            EntitySet entitySet = EntityDataModel.Current.EntitySets["Products"];

            ODataQueryOptions odataQueryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                entitySet,
                Mock.Of<IODataQueryOptionsValidator>());

            string odataContext = httpRequest.ResolveODataContext(entitySet, odataQueryOptions.Select);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products(*)", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySetAndSelectExpandQueryOptionAll_ReturnsContext_IfMetadataIsMinimal()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products?$select=*", ODataMetadataLevel.Minimal);

            EntitySet entitySet = EntityDataModel.Current.EntitySets["Products"];

            ODataQueryOptions odataQueryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                entitySet,
                Mock.Of<IODataQueryOptionsValidator>());

            string odataContext = httpRequest.ResolveODataContext(entitySet, odataQueryOptions.Select);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products(*)", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySetAndSelectExpandQueryOptionAll_ReturnsNull_IfMetadataIsNone()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products?$select=*", ODataMetadataLevel.None);

            EntitySet entitySet = EntityDataModel.Current.EntitySets["Products"];

            ODataQueryOptions odataQueryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                entitySet,
                Mock.Of<IODataQueryOptionsValidator>());

            string odataContext = httpRequest.ResolveODataContext(entitySet, odataQueryOptions.Select);

            Assert.Null(odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySetAndSelectExpandQueryOptionProperties_ReturnsContext_IfMetadataIsFull()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products?$select=Name,Price", ODataMetadataLevel.Full);

            EntitySet entitySet = EntityDataModel.Current.EntitySets["Products"];

            ODataQueryOptions odataQueryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                entitySet,
                Mock.Of<IODataQueryOptionsValidator>());

            string odataContext = httpRequest.ResolveODataContext(entitySet, odataQueryOptions.Select);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products(Name,Price)", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySetAndSelectExpandQueryOptionProperties_ReturnsContext_IfMetadataIsMinimal()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products?$select=Name,Price", ODataMetadataLevel.Minimal);

            EntitySet entitySet = EntityDataModel.Current.EntitySets["Products"];

            ODataQueryOptions odataQueryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                entitySet,
                Mock.Of<IODataQueryOptionsValidator>());

            string odataContext = httpRequest.ResolveODataContext(entitySet, odataQueryOptions.Select);

            Assert.Equal("https://services.odata.org/OData/$metadata#Products(Name,Price)", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataContext_WithEntitySetAndSelectExpandQueryOptionProperties_ReturnsNull_IfMetadataIsNone()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products?$select=Name,Price", ODataMetadataLevel.None);

            EntitySet entitySet = EntityDataModel.Current.EntitySets["Products"];

            ODataQueryOptions odataQueryOptions = new ODataQueryOptions(
                httpRequest.QueryString.Value,
                entitySet,
                Mock.Of<IODataQueryOptionsValidator>());

            string odataContext = httpRequest.ResolveODataContext(entitySet, odataQueryOptions.Select);

            Assert.Null(odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataId_WithIntEntityKey()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Orders");

            string odataContext = httpRequest.ResolveODataId(EntityDataModel.Current.EntitySets["Orders"], 12345);

            Assert.Equal("https://services.odata.org/OData/Orders(12345)", odataContext);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResolveODataId_WithStringEntityKey()
        {
            TestHelper.EnsureEDM();

            HttpRequest httpRequest = TestHelper.CreateODataHttpRequest("/OData/Products");

            string odataContext = httpRequest.ResolveODataId(EntityDataModel.Current.EntitySets["Products"], "Milk");

            Assert.Equal("https://services.odata.org/OData/Products('Milk')", odataContext);
        }
    }
}
