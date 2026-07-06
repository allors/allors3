// <copyright file="TestUserAuthenticationHandler.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    // Test-only credential: a request carrying the "X-Allors-TestUser" header is authenticated as that
    // user without a password (used by jest and the remote C# suites). This handler is registered only
    // in the abstract test-harness server's Startup, never in the inherited hosting seam, so it can
    // never reach a downstream inheritor's production build.
    public class TestUserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "AllorsTestUser";

        public const string HeaderName = "X-Allors-TestUser";

        private readonly UserManager<IdentityUser> userManager;

        public TestUserAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            UserManager<IdentityUser> userManager)
            : base(options, logger, encoder) =>
            this.userManager = userManager;

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!this.Request.Headers.TryGetValue(HeaderName, out var headerValues))
            {
                return AuthenticateResult.NoResult();
            }

            var userName = headerValues.ToString();
            var user = await this.userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return AuthenticateResult.Fail($"Unknown test user '{userName}'.");
            }

            // The same claims the JWT and Identity cookie emit, so the request resolves to the same
            // Allors user: TransactionService reads ClaimTypes.NameIdentifier as the user's object id.
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };
            var identity = new ClaimsIdentity(claims, this.Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
