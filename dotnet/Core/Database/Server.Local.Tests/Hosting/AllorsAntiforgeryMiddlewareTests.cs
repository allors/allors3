// <copyright file="AllorsAntiforgeryMiddlewareTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Allors.Server;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    // Antiforgery protects the Allors API against requests a browser sends with its cookie on its own.
    // An authentication plug-in names the schemes that sign in with such a cookie.
    public class AllorsAntiforgeryMiddlewareTests
    {
        [Fact]
        public async Task UnsafeRequestAuthenticatedByAListedSchemeIsValidated()
        {
            var antiforgery = new RecordingAntiforgery();

            await Invoke(Post("Tests.Cookie"), antiforgery, Options("Tests.Cookie"));

            Assert.True(antiforgery.Validated);
        }

        [Fact]
        public async Task UnsafeRequestAuthenticatedByAnUnlistedSchemeIsNotValidated()
        {
            var antiforgery = new RecordingAntiforgery();

            var nextCalled = await Invoke(Post("Tests.Header"), antiforgery, Options("Tests.Cookie"));

            Assert.False(antiforgery.Validated);
            Assert.True(nextCalled);
        }

        private static async Task<bool> Invoke(HttpContext context, IAntiforgery antiforgery, AllorsAntiforgeryOptions options)
        {
            var nextCalled = false;
            var middleware = new AllorsAntiforgeryMiddleware(
                _ =>
                {
                    nextCalled = true;
                    return Task.CompletedTask;
                },
                false);

            await middleware.InvokeAsync(context, antiforgery, Microsoft.Extensions.Options.Options.Create(options));
            return nextCalled;
        }

        private static AllorsAntiforgeryOptions Options(string authenticationType)
        {
            var options = new AllorsAntiforgeryOptions();
            options.AuthenticationTypes.Add(authenticationType);
            return options;
        }

        private static HttpContext Post(string authenticationType)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/allors/pull";
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "jane@example.com") }, authenticationType));
            return context;
        }

        private sealed class RecordingAntiforgery : IAntiforgery
        {
            public bool Validated { get; private set; }

            public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext) => new("request", "cookie", "form", "header");

            public AntiforgeryTokenSet GetTokens(HttpContext httpContext) => new("request", "cookie", "form", "header");

            public Task<bool> IsRequestValidAsync(HttpContext httpContext) => Task.FromResult(true);

            public Task ValidateRequestAsync(HttpContext httpContext)
            {
                this.Validated = true;
                return Task.CompletedTask;
            }

            public void SetCookieTokenAndHeader(HttpContext httpContext)
            {
            }
        }
    }
}
