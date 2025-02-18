// <copyright file="ExceptionHandler.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using NLog;
    using Microsoft.IdentityModel.Tokens;

    public static class ExceptionHandler
    {
        public static IApplicationBuilder ConfigureExceptionHandler(this IApplicationBuilder appBuilder, IHostingEnvironment env)
        {
            async Task Middleware(HttpContext context, Func<Task> next)
            {
                var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandler != null)
                {
                    var error = exceptionHandler.Error;

                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Error(error, "Unhandled Exception");

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = error is SecurityTokenExpiredException
                        ? (int)HttpStatusCode.Unauthorized
                        : (int)HttpStatusCode.InternalServerError;

                    var message = env.IsDevelopment() ?
                        $"{error.Message}\n{error.StackTrace}" :
                        $"{error.Message}";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(message));
                }
                else
                {
                    await next();
                }
            }

            return appBuilder.UseExceptionHandler(v => v.Use(Middleware));
        }
    }
}
