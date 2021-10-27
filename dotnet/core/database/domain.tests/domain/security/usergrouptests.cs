// <copyright file="UserGroupTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Configuration.Derivations.Default;
    using Xunit;

    public class UserGroupTests : DomainTest, IClassFixture<Fixture>
    {
        public UserGroupTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenNoUserGroupWhenCreatingAUserGroupWithoutANameThenUserGroupIsInvalid()
        {
            new UserGroupBuilder(this.Transaction).Build();

            var validation = this.Transaction.Derive(false);

            Assert.True(validation.HasErrors);
            Assert.Single(validation.Errors);

            var derivationError = validation.Errors[0];

            Assert.Single(derivationError.Relations);
            Assert.Equal(typeof(DerivationErrorRequired), derivationError.GetType());
            Assert.Equal(this.M.UserGroup.Name.RelationType, derivationError.Relations[0].RelationType);
        }
    }
}
