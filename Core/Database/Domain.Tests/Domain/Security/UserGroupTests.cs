// <copyright file="UserGroupTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Domain;
    using Derivations.Errors;
    using Xunit;

    public class UserGroupTests : DomainTest, IClassFixture<Fixture>
    {
        public UserGroupTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenNoUserGroupWhenCreatingAUserGroupWithoutANameThenUserGroupIsInvalid()
        {
            _ = new UserGroupBuilder(this.Transaction).Build();

            var validation = this.Transaction.Derive(false);

            Assert.True(validation.HasErrors);
            _ = Assert.Single(validation.Errors);

            var derivationError = validation.Errors[0];

            _ = Assert.Single(derivationError.Relations);
            Assert.Equal(typeof(DerivationErrorRequired), derivationError.GetType());
            Assert.Equal(this.M.UserGroup.Name.RelationType, derivationError.Relations[0].RelationType);
        }

        [Fact]
        public void GivenAUserGroupWhenCreatingAUserGroupWithTheSameNameThenUserGroupIsInvalid()
        {
            _ = new UserGroupBuilder(this.Transaction).WithName("Same").Build();
            _ = new UserGroupBuilder(this.Transaction).WithName("Same").Build();

            var validation = this.Transaction.Derive(false);

            Assert.True(validation.HasErrors);
            Assert.Equal(2, validation.Errors.Length);

            foreach (var derivationError in validation.Errors)
            {
                _ = Assert.Single(derivationError.Relations);
                Assert.Equal(typeof(DerivationErrorUnique), derivationError.GetType());
                Assert.Equal(this.M.UserGroup.Name.RelationType, derivationError.Relations[0].RelationType);
            }
        }
    }
}
