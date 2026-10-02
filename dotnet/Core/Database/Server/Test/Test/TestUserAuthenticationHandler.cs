// <copyright file="TestUserAuthenticationHandler.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Globalization;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Allors.Services;
    using Database.Domain;
    using Database.Meta;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    // Test-only credential: a request carrying the "X-Allors-TestUser" header is authenticated as the
    // user with that UniqueId, without a password (used by jest and the remote C# suites). The user is
    // looked up in the Allors database, so the test sign-in does not depend on an authentication
    // plug-in. This handler is registered only in the abstract test-harness server's Startup, never in
    // the inherited hosting seam, so it can never reach a downstream inheritor's production build.
    public class TestUserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "AllorsTestUser";

        public const string HeaderName = "X-Allors-TestUser";

        private readonly IDatabaseService databaseService;

        public TestUserAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IDatabaseService databaseService)
            : base(options, logger, encoder) =>
            this.databaseService = databaseService;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!this.Request.Headers.TryGetValue(HeaderName, out var headerValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var value = headerValues.ToString();
            if (!Guid.TryParse(value, out var uniqueId))
            {
                return Task.FromResult(AuthenticateResult.Fail($"Test user '{value}' is not a UniqueId: send the UniqueId of a user of the test population."));
            }

            using var transaction = this.databaseService.Database.CreateTransaction();
            var m = transaction.Database.Services.Get<MetaPopulation>();
            var user = new Users(transaction).FindBy(m.User.UniqueId, uniqueId);
            if (user == null)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Unknown test user '{value}'."));
            }

            // The name is the UniqueId the client sent; the name identifier is the Allors object id,
            // which the server's IUserResolver turns into the user.
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, uniqueId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            };
            var identity = new ClaimsIdentity(claims, this.Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
