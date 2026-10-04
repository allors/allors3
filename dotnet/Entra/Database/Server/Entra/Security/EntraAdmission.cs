// <copyright file="EntraAdmission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Database;
    using Database.Domain;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Services;
    using User = Database.Domain.User;

    // Admits a principal that Microsoft Entra ID has validated: the user of its identity is found, or
    // created at its first sign-in by the application's IUserFactory. The plug-in identifies every
    // principal by the pair of its tenant id and object id, a person's and a program's alike, and
    // never decides which class a new user is of, nor whom the application admits: the factory sees
    // the validated principal's claims and decides both. One instance serves a server, so that
    // parallel first requests of one principal create one user.
    public partial class EntraAdmission
    {
        // The role types of the strings the directory sends; a value longer than the role allows is
        // cut, as only the database would refuse it.
        private const int Size = 256;

        private readonly IDatabaseService databaseService;
        private readonly IUserFactory userFactory;
        private readonly ILogger<EntraAdmission> logger;
        private readonly object creating = new();

        public EntraAdmission(IDatabaseService databaseService, ILogger<EntraAdmission> logger = null, IUserFactory userFactory = null)
        {
            this.databaseService = databaseService;
            this.logger = logger ?? NullLogger<EntraAdmission>.Instance;
            this.userFactory = userFactory;
        }

        // Null when the principal is admitted, else why it is not; the reason is also logged. A
        // browser sign-in refreshes what the directory says about an existing user; a bearer token does
        // not, so that the fields are what the person last signed in with.
        public string Admit(ClaimsPrincipal principal, bool signIn)
        {
            var tenantId = principal.TenantId();
            var objectId = principal.ObjectId();
            if (tenantId == null || objectId == null)
            {
                this.LogNoIdentity(principal.UserName());
                return $"The token carries no Entra identity: both the {EntraClaims.TenantIdClaim} and the {EntraClaims.ObjectIdClaim} claim are required.";
            }

            var database = this.databaseService.Database;

            using (var transaction = database.CreateTransaction())
            {
                var user = new Users(transaction).FindByEntraIdentity(tenantId.Value, objectId.Value);
                if (user != null)
                {
                    if (signIn && this.Refresh(user, principal))
                    {
                        transaction.Derive();
                        transaction.Commit();
                    }

                    return null;
                }
            }

            if (this.userFactory == null)
            {
                this.LogNoUserFactory(objectId.Value);
                return $"No {nameof(IUserFactory)} is registered, so no user is created for a first sign-in. Register the user factory of the application's domain in Startup, for example services.AddSingleton<{nameof(IUserFactory)}, CustomUserFactory>().";
            }

            // One creation at a time: a parallel request of the same principal may have created the
            // user meanwhile, and finds it then.
            lock (this.creating)
            {
                using var transaction = database.CreateTransaction();
                try
                {
                    if (new Users(transaction).FindByEntraIdentity(tenantId.Value, objectId.Value) != null)
                    {
                        return null;
                    }

                    var user = this.userFactory.Create(transaction, principal);
                    if (user == null)
                    {
                        this.LogNotAdmitted(objectId.Value, principal.UserName());
                        return $"The {nameof(IUserFactory)} of the application did not admit the principal with object id {objectId}.";
                    }

                    // A factory that hands back an existing user would bind a second Entra identity to it,
                    // or move the identity of one user to another: a user is created for an identity, not
                    // chosen for it.
                    if (!user.Strategy.IsNewInTransaction)
                    {
                        this.LogExistingUser(objectId.Value, user.Id);
                        return $"The {nameof(IUserFactory)} of the application returned an existing user ({user.Id}) for the principal with object id {objectId}; a factory creates a new user or returns null.";
                    }

                    user.EntraTenantId = tenantId.Value;
                    user.EntraObjectId = objectId.Value;
                    this.Refresh(user, principal);

                    var validation = transaction.Derive(false);
                    if (validation.HasErrors)
                    {
                        var errors = string.Join("; ", validation.Errors.Select(v => v.Message));
                        this.LogDerivationFailed(objectId.Value, errors);
                        transaction.Rollback();
                        return $"The new user for the principal with object id {objectId} is not valid: {errors}";
                    }

                    transaction.Commit();
                    this.LogCreated(user.Id, user.Strategy.Class.Name, objectId.Value);
                    return null;
                }
                catch (Exception e)
                {
                    this.LogCreateFailed(e, objectId.Value);
                    return $"Could not create a user for the principal with object id {objectId}: {e.Message}";
                }
            }
        }

        // The principal that the browser session keeps: the Entra identity and the user name, and not
        // the token's other claims, so that the cookie stays small and carries nothing it need not.
        // Core's API reads the user name from the identity's name.
        public static ClaimsPrincipal SessionPrincipal(ClaimsPrincipal principal)
        {
            var claims = new List<Claim>
            {
                new(EntraClaims.TenantIdClaim, principal.TenantId()?.ToString() ?? string.Empty),
                new(EntraClaims.ObjectIdClaim, principal.ObjectId()?.ToString() ?? string.Empty),
            };

            var userName = principal.UserName();
            if (userName != null)
            {
                claims.Add(new Claim(EntraClaims.PreferredUserNameClaim, userName));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, principal.Identity?.AuthenticationType, EntraClaims.PreferredUserNameClaim, ClaimTypes.Role));
        }

        // The failure a sign-in fails with when the principal is not admitted, so that the plug-in can
        // tell it from the failures of the protocol when it answers the browser.
        public sealed class NotAdmittedException : Exception
        {
            public NotAdmittedException(string reason) : base(reason)
            {
            }
        }

        // Writes what the directory says about the account; true when a field changed.
        private bool Refresh(User user, ClaimsPrincipal principal)
        {
            var changed = false;
            changed |= Set(user.EntraUserName, this.Cut(principal.UserName(), nameof(user.EntraUserName)), v => user.EntraUserName = v);
            changed |= Set(user.EntraDisplayName, this.Cut(principal.DisplayName(), nameof(user.EntraDisplayName)), v => user.EntraDisplayName = v);
            changed |= Set(user.EntraEmail, this.Cut(principal.Email(), nameof(user.EntraEmail)), v => user.EntraEmail = v);
            changed |= Set(user.EntraIdentityProvider, this.Cut(principal.IdentityProvider(), nameof(user.EntraIdentityProvider)), v => user.EntraIdentityProvider = v);

            var isGuest = principal.IsGuest();
            if (user.EntraIsGuest != isGuest)
            {
                user.EntraIsGuest = isGuest;
                changed = true;
            }

            return changed;
        }

        private static bool Set(string current, string value, Action<string> set)
        {
            if (string.Equals(current, value, StringComparison.Ordinal))
            {
                return false;
            }

            set(value);
            return true;
        }

        private string Cut(string value, string field)
        {
            if (value == null || value.Length <= Size)
            {
                return value;
            }

            this.LogCut(field, value.Length, Size);
            return value[..Size];
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Created user {UserId} ({Class}) for the Entra principal with object id {ObjectId}.")]
        private partial void LogCreated(long userId, string @class, Guid objectId);

        [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Refused the principal {UserName}: the token carries no tid and oid claims.")]
        private partial void LogNoIdentity(string userName);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Could not create a user for the Entra principal with object id {ObjectId}: no IUserFactory is registered.")]
        private partial void LogNoUserFactory(Guid objectId);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "The IUserFactory of the application did not admit the Entra principal with object id {ObjectId} ({UserName}).")]
        private partial void LogNotAdmitted(Guid objectId, string userName);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "The IUserFactory of the application returned the existing user {UserId} for the Entra principal with object id {ObjectId}; a factory creates a new user or returns null.")]
        private partial void LogExistingUser(Guid objectId, long userId);

        [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "The new user for the Entra principal with object id {ObjectId} is not valid: {Errors}")]
        private partial void LogDerivationFailed(Guid objectId, string errors);

        [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Could not create a user for the Entra principal with object id {ObjectId}.")]
        private partial void LogCreateFailed(Exception exception, Guid objectId);

        [LoggerMessage(EventId = 8, Level = LogLevel.Warning, Message = "The directory sent {Field} with {Length} characters; the first {Size} are kept.")]
        private partial void LogCut(string field, int length, int size);
    }
}
