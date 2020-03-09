// -----------------------------------------------------------------------
// <copyright file="ODataExceptionFilter.cs" company="Project Contributors">
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Net.Http.OData;

namespace Net.Http.AspNetCore.OData
{
    /// <summary>
    /// An <see cref="IExceptionFilter"/> which returns the correct response for an <see cref="ODataException"/>.
    /// </summary>
    public sealed class ODataExceptionFilter : IExceptionFilter
    {
        /// <summary>
        /// Called after an action has thrown an System.Exception.
        /// </summary>
        /// <param name="context">The <see cref="ExceptionContext"/>.</param>
        public void OnException(ExceptionContext context)
        {
            if (context?.Exception is ODataException odataException)
            {
                context.Result = new ObjectResult(odataException.ToODataErrorContent()) { StatusCode = (int)odataException.StatusCode };
                context.ExceptionHandled = true;
            }
        }
    }
}
