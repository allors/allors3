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
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    // Antiforgery protects the Allors API against requests a browser sends with its cookie on its own.
    // An authentication plug-in names the scheme of its browser session, and a request which that
    // scheme authenticated needs a token.
    public class AllorsAntiforgeryMiddlewareTests
    {
        [Fact]
        public async Task UnsafeRequestAuthenticatedByTheSessionSchemeIsValidated()
        {
            var antiforgery = new RecordingAntiforgery();

            await Invoke(Post("Tests.Cookie"), antiforgery, Options("Tests.Cookie"));

            Assert.True(antiforgery.Validated);
        }

        [Fact]
        public async Task UnsafeRequestAuthenticatedByAnotherSchemeIsNotValidated()
        {
            var antiforgery = new RecordingAntiforgery();

            var nextCalled = await Invoke(Post("Tests.Header"), antiforgery, Options("Tests.Cookie"));

            Assert.False(antiforgery.Validated);
            Assert.True(nextCalled);
        }

        // A session that signed in with OpenID Connect and a bearer token carry the same identity
        // type, so the type cannot tell a browser from another client. The scheme that authenticated
        // the request can.
        [Fact]
        public async Task TheAuthenticatingSchemeDecidesNotTheTypeOfTheIdentity()
        {
            var session = new RecordingAntiforgery();
            var bearer = new RecordingAntiforgery();

            await Invoke(Post("Tests.Cookie", "AuthenticationTypes.Federation"), session, Options("Tests.Cookie"));
            await Invoke(Post("Tests.Bearer", "AuthenticationTypes.Federation"), bearer, Options("Tests.Cookie"));

            Assert.True(session.Validated);
            Assert.False(bearer.Validated);
        }

        // A server without a browser session, such as one that takes bearer tokens only.
        [Fact]
        public async Task UnsafeRequestIsNotValidatedWhenNoSessionSchemeIsNamed()
        {
            var antiforgery = new RecordingAntiforgery();

            var nextCalled = await Invoke(Post("Tests.Cookie"), antiforgery, Options(null));

            Assert.False(antiforgery.Validated);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task AnonymousUnsafeRequestIsNotValidated()
        {
            var antiforgery = new RecordingAntiforgery();
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/allors/pull";

            var nextCalled = await Invoke(context, antiforgery, Options("Tests.Cookie"));

            Assert.False(antiforgery.Validated);
            Assert.True(nextCalled);
        }

        private static async Task<bool> Invoke(HttpContext context, IAntiforgery antiforgery, AllorsAuthenticationOptions options)
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

        private static AllorsAuthenticationOptions Options(string sessionScheme) => new() { SessionScheme = sessionScheme };

        // An unsafe request to the API that the given scheme authenticated, as the authentication
        // middleware leaves it: the user, and the result that names the scheme.
        private static HttpContext Post(string scheme, string identityType = null)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/allors/pull";
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "jane@example.com") }, identityType ?? scheme));
            context.Features.Set<IAuthenticateResultFeature>(new AuthenticateResultFeature
            {
                AuthenticateResult = AuthenticateResult.Success(new AuthenticationTicket(context.User, scheme)),
            });
            return context;
        }

        private sealed class AuthenticateResultFeature : IAuthenticateResultFeature
        {
            public AuthenticateResult AuthenticateResult { get; set; }
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
