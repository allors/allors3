// <copyright file="ReadUserIdentity.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using Database;
    using Database.Domain;

    /// <summary>
    /// Captures an authenticated principal after the host has completed admission. Resolution uses
    /// the supplied read transaction and never opens an ambient request transaction or admits a user.
    /// </summary>
    public sealed class ReadUserIdentity
    {
        private readonly ClaimsPrincipal principal;
        private readonly IUserResolver resolver;

        private ReadUserIdentity(ClaimsPrincipal principal, IUserResolver resolver)
        {
            this.principal = principal;
            this.resolver = resolver;
        }

        public static ReadUserIdentity Capture(ClaimsPrincipal principal, IUserResolver resolver)
        {
            ArgumentNullException.ThrowIfNull(resolver);
            if (principal?.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("A workspace read requires an authenticated principal. Complete authentication and admission before capturing its identity.");
            }

            var captured = Copy(principal);
            if (captured.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("The authenticated principal must supply an authentication type that can be captured for the workspace read.");
            }

            return new ReadUserIdentity(captured, resolver);
        }

        // The read-scope owner guards this transaction and validates the resolved user. Resolvers
        // must be read-only, including any work outside the supplied transaction. Each invocation
        // gets its own claims so a resolver cannot change the identity of a later attempt.
        public User Resolve(ITransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            return this.resolver.Resolve(Copy(this.principal), transaction);
        }

        private static ClaimsPrincipal Copy(ClaimsPrincipal principal) =>
            new ClaimsPrincipal(principal.Identities.Select(Copy));

        // Reconstruct ordinary identities and claims rather than calling virtual Clone methods.
        // Bootstrap tokens and custom identity behavior are not inputs to user resolution.
        private static ClaimsIdentity Copy(ClaimsIdentity identity) =>
            new ClaimsIdentity(identity.Claims.Select(Copy), identity.AuthenticationType, identity.NameClaimType, identity.RoleClaimType)
            {
                Label = identity.Label,
                Actor = identity.Actor == null ? null : Copy(identity.Actor),
            };

        private static Claim Copy(Claim claim)
        {
            var copied = new Claim(claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer);
            foreach (var property in claim.Properties)
            {
                copied.Properties.Add(property.Key, property.Value);
            }

            return copied;
        }
    }
}
