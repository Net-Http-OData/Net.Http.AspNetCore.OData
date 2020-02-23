// -----------------------------------------------------------------------
// <copyright file="ODataQueryOptionsModelBinderProvider.cs" company="Project Contributors">
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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Net.Http.OData.Query;

namespace Net.Http.AspNetCore.OData
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for the <see cref="ODataQueryOptions"/>.
    /// </summary>
    public sealed class ODataQueryOptionsModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// Creates a <see cref="IModelBinder"/> based on <see cref="ModelBinderProviderContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ModelBinderProviderContext"/>.</param>
        /// <returns>An <see cref="IModelBinder"/>.</returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context?.Metadata.ModelType == typeof(ODataQueryOptions))
            {
                return new BinderTypeModelBinder(typeof(ODataQueryOptionsModelBinder));
            }

            return null;
        }
    }
}
