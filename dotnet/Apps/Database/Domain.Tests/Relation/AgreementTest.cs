// <copyright file="AgreementTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class AgreementTest : DomainTest, IClassFixture<Fixture>
    {
        public AgreementTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenClientAgreement_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new ClientAgreementBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(DateTimeFactory.CreateDate(2010, 12, 31));
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("client agreement");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenEmploymentAgreement_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new EmploymentAgreementBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(DateTimeFactory.CreateDate(2010, 12, 31));
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            this.Transaction.Rollback();

            builder.WithDescription("employment agreement");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenPurchaseAgreement_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PurchaseAgreementBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(DateTimeFactory.CreateDate(2010, 12, 31));
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("purchase agreement");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenSalesAgreement_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new SalesAgreementBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(DateTimeFactory.CreateDate(2010, 12, 31));
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("sales agreement");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenSubContractorAgreement_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new SubContractorAgreementBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithFromDate(DateTimeFactory.CreateDate(2010, 12, 31));
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("subContractor agreement");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
