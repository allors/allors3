// <copyright file="CreditCardTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CreditCardTests : DomainTest, IClassFixture<Fixture>
    {
        public CreditCardTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCreditCard_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new CreditCardBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCardNumber("4012888888881881");
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithExpirationYear(2016);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithExpirationMonth(03);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithNameOnCard("Name");
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build());
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenCreditCard_WhenDeriving_ThenCardNumberMustBeUnique()
        {
            new CreditCardBuilder(this.Transaction)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(2016)
                .WithExpirationMonth(03)
                .WithNameOnCard("Name")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build())
                .Build();

            Assert.False(this.Derive().HasErrors);

            new CreditCardBuilder(this.Transaction)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(2016)
                .WithExpirationMonth(03)
                .WithNameOnCard("Name")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build())
                .Build();

            Assert.True(this.Derive().HasErrors);
        }
    }
}
