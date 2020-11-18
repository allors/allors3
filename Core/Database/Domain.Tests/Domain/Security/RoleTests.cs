// <copyright file="RoleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.Derivations.Errors;
    using Xunit;

    public class RoleTests : DomainTest, IClassFixture<Fixture>
    {
        public RoleTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenNoRolesWhenCreatingARoleWithoutANameThenRoleIsInvalid()
        {
            new RoleBuilder(this.Session).Build();

            var validation = this.Session.Derive(false);

            Assert.True(validation.HasErrors);
            Assert.Single(validation.Errors);

            var derivationError = validation.Errors[0];

            Assert.Single(derivationError.Relations);
            Assert.Equal(typeof(DerivationErrorRequired), derivationError.GetType());
            Assert.Equal(this.M.Role.Name.RelationType, derivationError.Relations[0].RelationType);
        }

        [Fact]
        public void GivenARoleWhenCreatingARoleWithTheSameNameThenRoleIsInvalid()
        {
            new RoleBuilder(this.Session)
                .WithName("Same")
                .Build();

            new RoleBuilder(this.Session)
                .WithName("Same")
                .Build();

            var validation = this.Session.Derive(false);

            Assert.True(validation.HasErrors);
            Assert.Equal(2, validation.Errors.Length);

            foreach (var derivationError in validation.Errors)
            {
                Assert.Single(derivationError.Relations);
                Assert.Equal(typeof(DerivationErrorUnique), derivationError.GetType());
                Assert.Equal(this.M.Role.Name.RelationType, derivationError.Relations[0].RelationType);
            }
        }

        [Fact]
        public void GivenNoRolesWhenCreatingARoleWithoutAUniqueIdThenRoleIsValid()
        {
            var role = new RoleBuilder(this.Session)
                .WithName("Role")
                .Build();

            Assert.True(role.ExistUniqueId);

            var validation = this.Session.Derive(false);

            Assert.False(validation.HasErrors);
        }
    }
}
