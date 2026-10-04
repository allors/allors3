// <copyright file="EntraClaims.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System;
    using System.Linq;
    using System.Security.Claims;

    // The claims of a Microsoft Entra ID token, read from a validated principal. The handlers map
    // some claims to long names (tid to .../tenantid, for instance) unless an application switches
    // the mapping off, so every reader takes both names. An application's IUserFactory reads the
    // same claims to decide whom it admits and as what.
    public static class EntraClaims
    {
        public const string TenantIdClaim = "tid";
        public const string TenantIdMappedClaim = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string ObjectIdClaim = "oid";
        public const string ObjectIdMappedClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string PreferredUserNameClaim = "preferred_username";
        public const string UpnClaim = "upn";
        public const string NameClaim = "name";
        public const string EmailClaim = "email";
        public const string IdentityProviderClaim = "idp";
        public const string IdentityProviderMappedClaim = "http://schemas.microsoft.com/identity/claims/identityprovider";
        public const string IssuerClaim = "iss";
        public const string AccountTypeClaim = "acct";
        public const string IdentityTypeClaim = "idtyp";
        public const string ScopeClaim = "scp";
        public const string ScopeMappedClaim = "http://schemas.microsoft.com/identity/claims/scope";
        public const string RolesClaim = "roles";
        public const string AuthorizedPartyClaim = "azp";
        public const string ApplicationIdClaim = "appid";

        // The tenant the token was issued by, or null without a tid claim.
        public static Guid? TenantId(this ClaimsPrincipal principal) => AsGuid(principal.First(TenantIdClaim, TenantIdMappedClaim));

        // The object the token stands for in that tenant, a user or a service principal, or null.
        public static Guid? ObjectId(this ClaimsPrincipal principal) => AsGuid(principal.First(ObjectIdClaim, ObjectIdMappedClaim));

        // The user name: preferred_username of a v2.0 token, upn of a v1.0 token; null for a program.
        public static string UserName(this ClaimsPrincipal principal) => principal.First(PreferredUserNameClaim, UpnClaim, ClaimTypes.Upn);

        public static string DisplayName(this ClaimsPrincipal principal) => principal.First(NameClaim);

        public static string Email(this ClaimsPrincipal principal) => principal.First(EmailClaim, ClaimTypes.Email);

        // Where the account lives: the idp claim of a guest, else the issuer, which is the tenant's own.
        public static string IdentityProvider(this ClaimsPrincipal principal) =>
            principal.First(IdentityProviderClaim, IdentityProviderMappedClaim, IssuerClaim);

        // A guest of the tenant: the optional acct claim is 1 for a guest and 0 for a member. Without
        // the claim nobody is a guest, so an application that invites guests adds the claim to its
        // registration.
        public static bool IsGuest(this ClaimsPrincipal principal) => principal.First(AccountTypeClaim) == "1";

        // A program's own token is identified by the optional idtyp claim. Roles without scopes
        // do not distinguish programs: a person's ID token may also have application roles.
        public static bool IsApplication(this ClaimsPrincipal principal) => principal.First(IdentityTypeClaim) == "app";

        // The delegated permissions of a person's token.
        public static string[] Scopes(this ClaimsPrincipal principal) =>
            (principal.First(ScopeClaim, ScopeMappedClaim) ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // The application roles of the token.
        public static string[] Roles(this ClaimsPrincipal principal) =>
            principal.Claims.Where(v => v.Type == RolesClaim || v.Type == ClaimTypes.Role).Select(v => v.Value).ToArray();

        // The application that requested the token: azp in a v2.0 token, appid in a v1.0 token.
        public static string ClientApplicationId(this ClaimsPrincipal principal) => principal.First(AuthorizedPartyClaim, ApplicationIdClaim);

        private static string First(this ClaimsPrincipal principal, params string[] types) =>
            types.Select(principal.FindFirstValue).FirstOrDefault(v => !string.IsNullOrEmpty(v));

        private static Guid? AsGuid(string value) => Guid.TryParse(value, out var guid) && guid != Guid.Empty ? guid : null;
    }
}
