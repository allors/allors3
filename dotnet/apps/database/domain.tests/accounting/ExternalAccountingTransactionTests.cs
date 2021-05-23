// <copyright file="ExternalAccountingTransactionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ExternalAccountingTransactionTests : DomainTest, IClassFixture<Fixture>
    {
        public ExternalAccountingTransactionTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenTaxDue_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var partyFrom = new OrganisationBuilder(this.Transaction).WithName("party from").Build();
            var partyTo = new OrganisationBuilder(this.Transaction).WithName("party to").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new TaxDueBuilder(this.Transaction);
            var taxDue = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("taxdue");
            taxDue = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithEntryDate(this.Transaction.Now());
            taxDue = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithTransactionDate(this.Transaction.Now().AddYears(1));
            taxDue = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithFromParty(partyFrom);
            taxDue = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithToParty(partyTo);
            taxDue = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
