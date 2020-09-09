using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Net.Http.AspNetCore.OData.Tests.Integration
{
    public abstract class IntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected IntegrationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;

            HttpClient = _factory.CreateClient();
            HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        protected HttpClient HttpClient { get; }

        public void Dispose()
        {
            HttpClient.Dispose();
            _factory.Dispose();
        }
    }
}
