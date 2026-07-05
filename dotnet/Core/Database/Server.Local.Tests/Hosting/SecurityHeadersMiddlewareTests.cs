// <copyright file="SecurityHeadersMiddlewareTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Allors.Server;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class SecurityHeadersMiddlewareTests
    {
        [Fact]
        public async Task ResponsesCarryBaselineSecurityHeaders()
        {
            var context = CreateContext(out var responseFeature);
            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, Configuration());

            await middleware.InvokeAsync(context);
            await responseFeature.FireOnStartingAsync();

            Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
            Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
            Assert.Equal("camera=(), geolocation=(), microphone=()", context.Response.Headers["Permissions-Policy"]);
            Assert.Equal("frame-ancestors 'none'", context.Response.Headers["Content-Security-Policy"]);
        }

        [Fact]
        public async Task ContentSecurityPolicyIsConfigurable()
        {
            var context = CreateContext(out var responseFeature);
            var configuration = Configuration(new Dictionary<string, string>
            {
                ["Security:ContentSecurityPolicy"] = "default-src 'self'",
            });
            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, configuration);

            await middleware.InvokeAsync(context);
            await responseFeature.FireOnStartingAsync();

            Assert.Equal("default-src 'self'", context.Response.Headers["Content-Security-Policy"]);
        }

        [Fact]
        public async Task NextMiddlewareIsInvoked()
        {
            var context = CreateContext(out _);
            var nextInvoked = false;
            var middleware = new SecurityHeadersMiddleware(
                _ =>
                {
                    nextInvoked = true;
                    return Task.CompletedTask;
                },
                Configuration());

            await middleware.InvokeAsync(context);

            Assert.True(nextInvoked);
        }

        private static DefaultHttpContext CreateContext(out StartingCapableResponseFeature responseFeature)
        {
            var context = new DefaultHttpContext();
            responseFeature = new StartingCapableResponseFeature();
            context.Features.Set<IHttpResponseFeature>(responseFeature);
            return context;
        }

        private static IConfiguration Configuration(IDictionary<string, string> values = null) =>
            new ConfigurationBuilder().AddInMemoryCollection(values ?? new Dictionary<string, string>()).Build();

        private sealed class StartingCapableResponseFeature : HttpResponseFeature
        {
            private readonly List<(Func<object, Task> Callback, object State)> callbacks = new();

            public override void OnStarting(Func<object, Task> callback, object state) => this.callbacks.Add((callback, state));

            public async Task FireOnStartingAsync()
            {
                for (var i = this.callbacks.Count - 1; i >= 0; i--)
                {
                    await this.callbacks[i].Callback(this.callbacks[i].State);
                }
            }
        }
    }
}
