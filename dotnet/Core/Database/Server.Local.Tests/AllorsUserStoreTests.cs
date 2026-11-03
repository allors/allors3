// <copyright file="AllorsUserStoreTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Threading.Tasks;
    using Allors.Database.Domain;
    using Allors.Security;
    using Allors.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class AllorsUserStoreTests : ApiTest, IClassFixture<Fixture>
    {
        public AllorsUserStoreTests(Fixture fixture) : base(fixture, populate: false)
        {
        }

        [Fact]
        public async Task FailedSignInDoesNotOverwritePasswordReset()
        {
            var person = new PersonBuilder(this.Transaction).WithUserName("Jane").Build();
            person.UserLockoutEnabled = true;
            person.SetPassword("Old-p@ssword1");
            this.Transaction.Derive();
            this.Transaction.Commit();

            using var services = this.CreateIdentityServices();
            using var scope = services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var signIns = scope.ServiceProvider.GetRequiredService<SignInManager<IdentityUser>>();
            var identity = await users.FindByNameAsync("Jane");

            person.SetPassword("New-p@ssword2");
            this.Transaction.Derive();
            var resetStamp = person.UserSecurityStamp;
            this.Transaction.Commit();

            var result = await signIns.CheckPasswordSignInAsync(identity, "wrong", lockoutOnFailure: true);

            Assert.False(result.Succeeded);
            this.Transaction.Rollback();
            Assert.True(person.VerifyPassword("New-p@ssword2"));
            Assert.False(person.VerifyPassword("Old-p@ssword1"));
            Assert.Equal(resetStamp, person.UserSecurityStamp);
            Assert.Equal(0, person.UserAccessFailedCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task PasswordChangeAllowsFurtherIdentityUpdates(bool createWithIdentity)
        {
            using var services = this.CreateIdentityServices();
            using var scope = services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            IdentityUser identity;

            if (createWithIdentity)
            {
                identity = new IdentityUser("Jane");
                var created = await users.CreateAsync(identity);
                Assert.True(created.Succeeded);
                identity.PasswordHash = users.PasswordHasher.HashPassword(identity, "Old-p@ssword1");
                var initialized = await users.UpdateAsync(identity);
                Assert.True(initialized.Succeeded);
            }
            else
            {
                var person = new PersonBuilder(this.Transaction).WithUserName("Jane").Build();
                person.SetPassword("Old-p@ssword1");
                this.Transaction.Derive();
                this.Transaction.Commit();
                identity = await users.FindByNameAsync("Jane");
            }

            var originalStamp = identity.SecurityStamp;
            var changed = await users.ChangePasswordAsync(identity, "Old-p@ssword1", "New-p@ssword2");
            Assert.True(changed.Succeeded);
            Assert.NotEqual(originalStamp, identity.SecurityStamp);

            var failedAttempt = await users.AccessFailedAsync(identity);
            Assert.True(failedAttempt.Succeeded);

            this.Transaction.Rollback();
            var updated = (Person)this.Transaction.Instantiate(identity.Id);
            Assert.True(updated.VerifyPassword("New-p@ssword2"));
            Assert.False(updated.VerifyPassword("Old-p@ssword1"));
            Assert.Equal(identity.SecurityStamp, updated.UserSecurityStamp);
            Assert.Equal(1, updated.UserAccessFailedCount);
        }

        private ServiceProvider CreateIdentityServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication();
            services.AddIdentityCore<IdentityUser>().AddSignInManager();
            services.AddSingleton<IDatabaseService>(new DatabaseService { Database = this.Transaction.Database });
            services.AddScoped<IUserStore<IdentityUser>, AllorsUserStore>();
            return services.BuildServiceProvider();
        }
    }
}
