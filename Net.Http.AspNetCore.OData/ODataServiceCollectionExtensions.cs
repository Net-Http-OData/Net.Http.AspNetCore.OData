// -----------------------------------------------------------------------
// <copyright file="ODataServiceCollectionExtensions.cs" company="Project Contributors">
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
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Net.Http.AspNetCore.OData;
using Net.Http.OData.Model;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Extension methods for setting up OData services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ODataServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OData services with the specified Entity Data Model with <see cref="StringComparer"/>.OrdinalIgnoreCase for the model name matching.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="entityDataModelBuilderCallback">The call-back to configure the Entity Data Model.</param>
        public static void AddOData(
            this IServiceCollection services,
            Action<EntityDataModelBuilder> entityDataModelBuilderCallback)
            => AddOData(services, entityDataModelBuilderCallback, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds OData services with the specified Entity Data Model and equality comparer for the model name matching.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="entityDataModelBuilderCallback">The call-back to configure the Entity Data Model.</param>
        /// <param name="entitySetNameComparer">The comparer to use for the entty set name matching.</param>
        public static void AddOData(
            this IServiceCollection services,
            Action<EntityDataModelBuilder> entityDataModelBuilderCallback,
            IEqualityComparer<string> entitySetNameComparer)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (entityDataModelBuilderCallback is null)
            {
                throw new ArgumentNullException(nameof(entityDataModelBuilderCallback));
            }

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new ODataExceptionFilter());

                options.ModelBinderProviders.Insert(0, new ODataQueryOptionsModelBinderProvider());
            });

            var entityDataModelBuilder = new EntityDataModelBuilder(entitySetNameComparer);
            entityDataModelBuilderCallback(entityDataModelBuilder);
            entityDataModelBuilder.BuildModel();
        }
    }
}
