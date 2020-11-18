// <copyright file="GeneralLedgerAccountTypeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class GeneralLedgerAccountTypeTests : DomainTest, IClassFixture<Fixture>
    {
        public GeneralLedgerAccountTypeTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenGeneralLedgerAccountType_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new GeneralLedgerAccountTypeBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithDescription("GeneralLedgerAccountType");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }
    }
}
