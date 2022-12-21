// <copyright file="PartyFinancialRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class PartyFinancialRelationshipFromDateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyFinancialRelationshipFromDateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void PeriodActiveThrowValidationError()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(1)).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodActiveThrowValidationError_1()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(-1)).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodActiveThrowValidationError_2()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithThroughDate(this.Transaction.Now().AddDays(1)).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(1)).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodNotActive()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithThroughDate(this.Transaction.Now().AddDays(1)).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(2)).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void PeriodNotActive_1()
        {
            var customer = new PersonBuilder(this.Transaction).WithLastName("customer").Build();
            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new PartyFinancialRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(-2)).WithThroughDate(this.Transaction.Now().AddDays(-1)).WithFinancialParty(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
