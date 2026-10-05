// <copyright file="AllorsUserStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Database;
    using Database.Domain;
    using Database.Meta;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Services;
    using Deletable = Database.Domain.Deletable;
    using Task = System.Threading.Tasks.Task;
    using User = Database.Domain.User;

    public partial class AllorsUserStore : IUserPasswordStore<IdentityUser>,
                                           IUserLoginStore<IdentityUser>,
                                           IUserSecurityStampStore<IdentityUser>,
                                           IUserTwoFactorStore<IdentityUser>,
                                           IUserEmailStore<IdentityUser>,
                                           IUserLockoutStore<IdentityUser>,
                                           IUserPhoneNumberStore<IdentityUser>
    {
        private readonly IDatabase database;
        private readonly ILogger<AllorsUserStore> logger;
        private readonly IUserFactory userFactory;

        public AllorsUserStore(IDatabaseService databaseService, ILogger<AllorsUserStore> logger = null, IUserFactory userFactory = null)
        {
            this.database = databaseService.Database;
            this.logger = logger ?? NullLogger<AllorsUserStore>.Instance;
            this.userFactory = userFactory;
        }

        #region IUserStore
        public void Dispose()
        {
        }

        public async Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.Id;
        }

        public async Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.UserName;
        }

        public async Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.UserName = userName;
        }

        public async Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.NormalizedUserName;
        }

        public async Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedUserName = normalizedName;
        }

        // The plug-in knows a user as an interface only, so it creates none itself: the application's
        // user factory does, and the store then writes the fields of ASP.NET Core Identity on it.
        public async Task<IdentityResult> CreateAsync(IdentityUser identityUser, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.userFactory == null)
            {
                this.LogNoUserFactory(identityUser.UserName);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NoUserFactory",
                    Description = $"Could not create user {identityUser.UserName}: no {nameof(IUserFactory)} is registered. Register the user factory of the application's domain in Startup, for example services.AddSingleton<{nameof(IUserFactory)}, CustomUserFactory>().",
                });
            }

            using (var transaction = this.database.CreateTransaction())
            {
                try
                {
                    var user = this.userFactory.Create(transaction, Principal(identityUser));
                    if (user == null)
                    {
                        this.LogUserNotAdmitted(identityUser.UserName);
                        return IdentityResult.Failed(new IdentityError
                        {
                            Code = "UserNotAdmitted",
                            Description = $"Could not create user {identityUser.UserName}: the {nameof(IUserFactory)} of the application did not admit it.",
                        });
                    }

                    // Reusing a user here would replace their credentials with the incoming identity's.
                    if (!user.Strategy.IsNewInTransaction)
                    {
                        this.LogExistingUser(identityUser.UserName, user.Id);
                        return IdentityResult.Failed(new IdentityError
                        {
                            Code = "ExistingUserFromFactory",
                            Description = $"Could not create user {identityUser.UserName}: the {nameof(IUserFactory)} returned an existing user. Change the factory to return a new user or null.",
                        });
                    }

                    user.UserName = identityUser.UserName;
                    user.UserPasswordHash = identityUser.PasswordHash;
                    user.UserEmail = identityUser.Email;
                    user.UserEmailConfirmed = identityUser.EmailConfirmed;
                    user.UserPhoneNumber = identityUser.PhoneNumber;
                    user.UserPhoneNumberConfirmed = identityUser.PhoneNumberConfirmed;
                    user.UserTwoFactorEnabled = identityUser.TwoFactorEnabled;
                    user.UserLockoutEnd = identityUser.LockoutEnd?.UtcDateTime;
                    user.UserAccessFailedCount = identityUser.AccessFailedCount;

                    // The build hooks of the Identity domain ran when the factory built the user: it
                    // has a security stamp, and lockout is enabled. Keep both unless Identity has a
                    // stamp of its own for the user.
                    if (!string.IsNullOrEmpty(identityUser.SecurityStamp))
                    {
                        user.UserSecurityStamp = identityUser.SecurityStamp;
                    }

                    transaction.Derive();
                    transaction.Commit();

                    identityUser.Id = user.Id.ToString();

                    return IdentityResult.Success;
                }
                catch (Exception e)
                {
                    this.LogCreateFailed(e, identityUser.UserName);
                    return IdentityResult.Failed(new IdentityError { Description = $"Could not create user {identityUser.UserName}." });
                }
            }
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser identityUser, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var transaction = this.database.CreateTransaction())
            {
                try
                {
                    var user = identityUser.User(transaction);

                    user.UserName = identityUser.UserName;
                    user.UserPasswordHash = identityUser.PasswordHash;
                    user.UserEmail = identityUser.Email;
                    user.UserEmailConfirmed = identityUser.EmailConfirmed;
                    user.UserSecurityStamp = identityUser.SecurityStamp;
                    user.UserPhoneNumber = identityUser.PhoneNumber;
                    user.UserPhoneNumberConfirmed = identityUser.PhoneNumberConfirmed;
                    user.UserTwoFactorEnabled = identityUser.TwoFactorEnabled;
                    user.UserLockoutEnd = identityUser.LockoutEnd?.UtcDateTime;
                    user.UserLockoutEnabled = identityUser.LockoutEnabled;
                    user.UserAccessFailedCount = identityUser.AccessFailedCount;

                    transaction.Derive();
                    transaction.Commit();

                    return IdentityResult.Success;
                }
                catch (Exception e)
                {
                    this.LogUpdateFailed(e, identityUser.UserName);
                    return IdentityResult.Failed(new IdentityError { Description = $"Could not update user {identityUser.UserName}." });
                }
            }
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser identityUser, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var transaction = this.database.CreateTransaction())
            {
                try
                {
                    var user = (User)transaction.Instantiate(identityUser.Id);

                    if (user is Deletable deletable)
                    {
                        deletable.Delete();
                    }

                    transaction.Derive();
                    transaction.Commit();

                    return IdentityResult.Success;
                }
                catch (Exception e)
                {
                    this.LogDeleteFailed(e, identityUser.UserName);
                    return IdentityResult.Failed(new IdentityError { Description = $"Could not delete user {identityUser.UserName}." });
                }
            }
        }

        public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var transaction = this.database.CreateTransaction())
            {
                var user = (User)transaction.Instantiate(userId);
                return user?.AsIdentityUser();
            }
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var m = this.database.Services.Get<MetaPopulation>();

            cancellationToken.ThrowIfCancellationRequested();
            using (var transaction = this.database.CreateTransaction())
            {
                var user = new Users(transaction).FindBy(m.User.NormalizedUserName, normalizedUserName);
                return user?.AsIdentityUser();
            }
        }

        #endregion

        #region IUserPasswordStore
        public async Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.PasswordHash = passwordHash;
        }

        public async Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.PasswordHash;
        }

        public async Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return !string.IsNullOrWhiteSpace(user.PasswordHash);
        }
        #endregion

        #region IUserLoginStore
        public async Task AddLoginAsync(IdentityUser identityUser, UserLoginInfo userLoginInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var transaction = this.database.CreateTransaction())
            {
                var user = (User)transaction.Instantiate(identityUser.Id);

                var login = new LoginBuilder(transaction)
                    .WithProvider(userLoginInfo.LoginProvider)
                    .WithKey(userLoginInfo.ProviderKey)
                    .WithDisplayName(userLoginInfo.ProviderDisplayName)
                    .Build();

                user.AddLogin(login);

                transaction.Derive();
                transaction.Commit();
            }
        }

        public async Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var m = this.database.Services.Get<MetaPopulation>();

            using (var transaction = this.database.CreateTransaction())
            {
                var extent = new Logins(transaction).Extent();
                extent.Filter.AddEquals(m.Login.Provider, loginProvider);
                extent.Filter.AddEquals(m.Login.Key, providerKey);

                var user = extent.FirstOrDefault()?.UserWhereLogin;
                return user?.AsIdentityUser();
            }
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser identityUser, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var transaction = this.database.CreateTransaction())
            {
                var user = (User)transaction.Instantiate(identityUser.Id);
                return user.Logins.Select(v => v.AsUserLoginInfo()).ToArray();
            }
        }

        public async Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var m = this.database.Services.Get<MetaPopulation>();

            using (var transaction = this.database.CreateTransaction())
            {
                var extent = new Logins(transaction).Extent();
                extent.Filter.AddEquals(m.Login.Provider, loginProvider);
                extent.Filter.AddEquals(m.Login.Key, providerKey);

                var login = extent.FirstOrDefault();
                login?.Delete();

                transaction.Derive();
                transaction.Commit();
            }
        }

        #endregion

        #region IUserSecurityStampStore
        public async Task<string> GetSecurityStampAsync(IdentityUser identityUser, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return identityUser.SecurityStamp;
        }

        public async Task SetSecurityStampAsync(IdentityUser identityUser, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            identityUser.SecurityStamp = stamp;
        }

        #endregion

        #region IUserTwoFactorStore
        public async Task<bool> GetTwoFactorEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.TwoFactorEnabled;
        }

        public async Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.TwoFactorEnabled = enabled;
        }

        #endregion

        #region IUserEmailStore
        public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var m = this.database.Services.Get<MetaPopulation>();

            using (var transaction = this.database.CreateTransaction())
            {
                var user = new Users(transaction).FindBy(m.User.NormalizedUserEmail, normalizedEmail);
                return user?.AsIdentityUser();
            }
        }

        public async Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.Email;
        }

        public async Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.EmailConfirmed;
        }

        public async Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.NormalizedEmail;
        }

        public async Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.Email = email;
        }

        public async Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.EmailConfirmed = confirmed;
        }

        public async Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedEmail = normalizedEmail;
        }
        #endregion

        #region IUserLockoutStore
        public async Task<int> GetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.AccessFailedCount;
        }

        public async Task<bool> GetLockoutEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.LockoutEnabled;
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.LockoutEnd;
        }

        public async Task<int> IncrementAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ++user.AccessFailedCount;
        }

        public async Task ResetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.AccessFailedCount = default;
        }

        public async Task SetLockoutEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.LockoutEnabled = enabled;
        }

        public async Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.LockoutEnd = lockoutEnd;
        }

        #endregion

        #region IUserPhoneNumberStore
        public async Task<string> GetPhoneNumberAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.PhoneNumber;
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user.PhoneNumberConfirmed;
        }

        public async Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.PhoneNumber = phoneNumber;
        }

        public async Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.PhoneNumberConfirmed = confirmed;
        }

        #endregion

        // What ASP.NET Core Identity knows about the person it asks the store to create. Nobody signed
        // in, so the identity has no authentication type.
        private static ClaimsPrincipal Principal(IdentityUser identityUser)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(identityUser.UserName))
            {
                claims.Add(new Claim(ClaimTypes.Name, identityUser.UserName));
            }

            if (!string.IsNullOrEmpty(identityUser.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, identityUser.Email));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Could not create user {UserName}.")]
        private partial void LogCreateFailed(Exception exception, string userName);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Could not update user {UserName}.")]
        private partial void LogUpdateFailed(Exception exception, string userName);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Could not delete user {UserName}.")]
        private partial void LogDeleteFailed(Exception exception, string userName);

        [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Could not create user {UserName}: no IUserFactory is registered.")]
        private partial void LogNoUserFactory(string userName);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Could not create user {UserName}: the IUserFactory of the application did not admit it.")]
        private partial void LogUserNotAdmitted(string userName);

        [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Could not create user {UserName}: the IUserFactory returned the existing user {UserId}; a factory must return a new user or null.")]
        private partial void LogExistingUser(string userName, long userId);
    }
}
