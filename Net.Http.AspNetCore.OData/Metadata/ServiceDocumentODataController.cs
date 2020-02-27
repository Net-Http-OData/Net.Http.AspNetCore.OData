// -----------------------------------------------------------------------
// <copyright file="ServiceDocumentODataController.cs" company="Project Contributors">
// Copyright Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.Http.OData;
using Net.Http.OData.Metadata;
using Net.Http.OData.Model;

namespace Net.Http.AspNetCore.OData.Metadata
{
    /// <summary>
    /// An API controller which exposes the OData service document.
    /// </summary>
    [ApiController]
    [Route("odata")]
    public sealed class ServiceDocumentODataController : ControllerBase
    {
        /// <summary>
        /// Gets the <see cref="IActionResult"/> which contains the service document.
        /// </summary>
        /// <returns>The <see cref="IActionResult"/> which contains the service document.</returns>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            string odataContext = Request.ResolveODataContext();
            ODataRequestOptions odataRequestOptions = Request.ReadODataRequestOptions();
            IEnumerable<ServiceDocumentItem> serviceDocumentItems = ServiceDocumentProvider.Create(EntityDataModel.Current, odataRequestOptions);

            var odataResponseContent = new ODataResponseContent
            {
                Context = odataContext,
                Value = serviceDocumentItems,
            };

            return Ok(odataResponseContent);
        }
    }
}
