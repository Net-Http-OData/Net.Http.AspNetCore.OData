﻿using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Net.Http.OData;
using Net.Http.OData.Model;
using NorthwindModel;

namespace Net.Http.AspNetCore.OData.Tests
{
    internal static class TestHelper
    {
        /// <summary>
        /// Creates an <see cref="HttpRequest"/> (without ODataRequestOptions) for the URI 'https://services.odata.org/{path}'.
        /// </summary>
        /// <param name="pathAndQuery">The path for the request URI.</param>
        /// <returns>The HttpRequest without ODataRequestOptions.</returns>
        internal static HttpRequest CreateHttpRequest(string pathAndQuery)
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Headers["Accept"] = "application/json";
            httpContext.Response.Headers["Content-Type"] = "application/json";
            httpContext.Response.Body = new MemoryStream();

            int questionMark = pathAndQuery.IndexOf('?');
            string path = questionMark < 0 ? pathAndQuery : pathAndQuery.Substring(0, questionMark);
            string query = questionMark < 0 ? null : pathAndQuery.Substring(questionMark);

            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("services.odata.org");
            httpContext.Request.Path = new PathString(path);
            httpContext.Request.QueryString = query == null ? QueryString.Empty : new QueryString(query);

            return httpContext.Request;
        }

        /// <summary>
        /// Creates a <see cref="HttpRequest"/> (with ODataRequestOptions) for the URI 'https://services.odata.org/{path}'.
        /// </summary>
        /// <param name="pathAndQuery">The path for the request URI.</param>
        /// <param name="metadataLevel">The metadata level to use (defaults to minimal if not specified).</param>
        /// <returns>The HttpRequest</returns>
        internal static HttpRequest CreateODataHttpRequest(string pathAndQuery, ODataMetadataLevel metadataLevel = ODataMetadataLevel.Minimal)
        {
            HttpRequest httpRequest = CreateHttpRequest(pathAndQuery);
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
