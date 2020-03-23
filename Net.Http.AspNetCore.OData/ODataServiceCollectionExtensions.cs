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
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Net.Http.AspNetCore.OData;
using Net.Http.OData.Model;
using Net.Http.OData.Query.Parsers;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Extension methods for setting up OData services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ODataServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OData services with the specified Entity Data Model with <see cref="DateTimeStyles.AssumeUniversal"/>
        /// for parsing <see cref="DateTimeOffset"/>s, and <see cref="StringComparer"/>.OrdinalIgnoreCase for the model name matching.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="entityDataModelBuilderCallback">The call-back to configure the Entity Data Model.</param>
        public static void AddOData(
            this IServiceCollection services,
            Action<EntityDataModelBuilder> entityDataModelBuilderCallback)
            => AddOData(services, entityDataModelBuilderCallback, ParserSettings.DateTimeStyles);

        /// <summary>
        /// Adds OData services with the specified Entity Data Model with the specified <see cref="DateTimeStyles"/>
        /// for parsing <see cref="DateTimeOffset"/>s, and <see cref="StringComparer"/>.OrdinalIgnoreCase for the model name matching.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="entityDataModelBuilderCallback">The call-back to configure the Entity Data Model.</param>
        /// <param name="dateTimeOffsetParserStyle">The <see cref="DateTimeStyles"/> to use for parsing <see cref="DateTimeOffset"/> if no timezone is specified in the OData query.</param>
        public static void AddOData(
            this IServiceCollection services,
            Action<EntityDataModelBuilder> entityDataModelBuilderCallback,
            DateTimeStyles dateTimeOffsetParserStyle)
            => AddOData(services, entityDataModelBuilderCallback, dateTimeOffsetParserStyle, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds OData services with the specified Entity Data Model with the specified <see cref="DateTimeStyles"/>
        /// for parsing <see cref="DateTimeOffset"/>s, and <see cref="IEqualityComparer{T}"/> for the model name matching.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="entityDataModelBuilderCallback">The call-back to configure the Entity Data Model.</param>
        /// <param name="dateTimeOffsetParserStyle">The <see cref="DateTimeStyles"/> to use for parsing <see cref="DateTimeOffset"/> if no timezone is specified in the OData query.</param>
        /// <param name="entitySetNameComparer">The comparer to use for the entty set name matching.</param>
        public static void AddOData(
            this IServiceCollection services,
            Action<EntityDataModelBuilder> entityDataModelBuilderCallback,
            DateTimeStyles dateTimeOffsetParserStyle,
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

            ParserSettings.DateTimeStyles = dateTimeOffsetParserStyle;

            var entityDataModelBuilder = new EntityDataModelBuilder(entitySetNameComparer);
            entityDataModelBuilderCallback(entityDataModelBuilder);
            entityDataModelBuilder.BuildModel();
        }
    }
}
