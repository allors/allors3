// <copyright file="RoleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Configuration.Derivations.Default;
    using Xunit;

    public class RoleTests : DomainTest, IClassFixture<Fixture>
    {
        public RoleTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenNoRolesWhenCreatingARoleWithoutANameThenRoleIsInvalid()
        {
            new RoleBuilder(this.Transaction).Build();

            var validation = this.Transaction.Derive(false);

            Assert.True(validation.HasErrors);
            Assert.Single(validation.Errors);

            var derivationError = validation.Errors[0];

            Assert.Single(derivationError.Relations);
            Assert.Equal(typeof(DerivationErrorRequired), derivationError.GetType());
            Assert.Equal(this.M.Role.Name.RelationType, derivationError.Relations[0].RelationType);
        }

        [Fact]
        public void GivenNoRolesWhenCreatingARoleWithoutAUniqueIdThenRoleIsValid()
        {
            var role = new RoleBuilder(this.Transaction)
                .WithName("Role")
                .Build();

            Assert.True(role.ExistUniqueId);

            var validation = this.Transaction.Derive(false);

            Assert.False(validation.HasErrors);
        }
    }
}
