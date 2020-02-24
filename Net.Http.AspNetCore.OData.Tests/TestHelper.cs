using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Moq;
using Net.Http.OData;
using Net.Http.OData.Model;
using NorthwindModel;

namespace Net.Http.AspNetCore.OData.Tests
{
    internal static class TestHelper
    {
        /// <summary>
        /// Creates a <see cref="HttpRequest"/> for the URI 'https://services.odata.org/{path}'.
        /// </summary>
        /// <param name="pathAndQuery">The path for the request URI.</param>
        /// <param name="metadataLevel">The metadata level to use (defaults to minimal if not specified).</param>
        /// <returns>The HttpRequest</returns>
        internal static HttpRequest CreateHttpRequest(string pathAndQuery, ODataMetadataLevel metadataLevel = ODataMetadataLevel.Minimal)
        {
            var httpContextItems = new Dictionary<object, object>();

            var mockHttpContext = new Mock<HttpContext>();
            var mockHttpRequest = new Mock<HttpRequest>();

            mockHttpContext.Setup(x => x.Items).Returns(httpContextItems);
            mockHttpContext.Setup(x => x.Request).Returns(mockHttpRequest.Object);

            mockHttpRequest.SetupAllProperties();
            mockHttpRequest.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

            int questionMark = pathAndQuery.IndexOf('?');
            string path = questionMark < 0 ? pathAndQuery : pathAndQuery.Substring(0, questionMark);
            string query = questionMark < 0 ? null : pathAndQuery.Substring(questionMark);

            HttpRequest httpRequest = mockHttpRequest.Object;
            httpRequest.Scheme = "https";
            httpRequest.Host = new HostString("services.odata.org");
            httpRequest.Path = new PathString(path);
            httpRequest.QueryString = query == null ? QueryString.Empty : new QueryString(query);
            httpRequest.HttpContext.Items.Add(typeof(ODataRequestOptions).FullName, new ODataRequestOptions(new Uri("https://services.odata.org/OData/"), ODataIsolationLevel.None, metadataLevel, ODataVersion.OData40));

            return httpRequest;
        }

        internal static void EnsureEDM()
        {
            var entityDataModelBuilder = new EntityDataModelBuilder(StringComparer.OrdinalIgnoreCase)
                .RegisterEntitySet<Category>("Categories", x => x.Name, Capabilities.Insertable | Capabilities.Updatable | Capabilities.Deletable)
                .RegisterEntitySet<Customer>("Customers", x => x.CompanyName, Capabilities.Updatable)
                .RegisterEntitySet<Employee>("Employees", x => x.Id)
                .RegisterEntitySet<Manager>("Managers", x => x.Id)
                .RegisterEntitySet<Order>("Orders", x => x.OrderId, Capabilities.Insertable | Capabilities.Updatable)
                .RegisterEntitySet<Product>("Products", x => x.ProductId, Capabilities.Insertable | Capabilities.Updatable);

            entityDataModelBuilder.BuildModel();
        }
    }
}
