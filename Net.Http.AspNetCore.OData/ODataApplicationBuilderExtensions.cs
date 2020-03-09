// -----------------------------------------------------------------------
// <copyright file="ODataApplicationBuilderExtensions.cs" company="Project Contributors">
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
using Microsoft.AspNetCore.Builder;
using Net.Http.OData;

namespace Net.Http.AspNetCore.OData
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> to add OData to the request execution pipeline.
    /// </summary>
    public static class ODataApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="ODataRequestMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> with the added <see cref="ODataRequestMiddleware"/>.</returns>
        public static IApplicationBuilder UseOData(this IApplicationBuilder builder)
        {
            ODataServiceOptions.Current = new ODataServiceOptions(
                ODataVersion.MinVersion,
                ODataVersion.MaxVersion,
                new[] { ODataIsolationLevel.None },
                new[] { "application/json", "text/plain" });

            return builder.UseMiddleware<ODataRequestMiddleware>(ODataServiceOptions.Current);
        }
    }
}
