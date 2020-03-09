// -----------------------------------------------------------------------
// <copyright file="ODataQueryOptionsModelBinder.cs" company="Project Contributors">
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Net.Http.OData;
using Net.Http.OData.Model;
using Net.Http.OData.Query;

namespace Net.Http.AspNetCore.OData
{
    /// <summary>
    /// An <see cref="IModelBinder"/> for the <see cref="ODataQueryOptions"/>.
    /// </summary>
    internal sealed class ODataQueryOptionsModelBinder : IModelBinder
    {
        /// <summary>
        /// Attempts to bind a model.
        /// </summary>
        /// <param name="bindingContext">The <see cref="ModelBindingContext"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete when the model binding process completes.</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext != null)
            {
                HttpRequest request = bindingContext.HttpContext.Request;

                string query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
                EntitySet entitySet = request.ResolveEntitySet();
                ODataRequestOptions odataRequestOptions = request.ReadODataRequestOptions();
                IODataQueryOptionsValidator validator = ODataQueryOptionsValidator.GetValidator(odataRequestOptions.ODataVersion);

                var queryOptions = new ODataQueryOptions(query, entitySet, validator);

                bindingContext.Result = ModelBindingResult.Success(queryOptions);
            }

            return Task.CompletedTask;
        }
    }
}
