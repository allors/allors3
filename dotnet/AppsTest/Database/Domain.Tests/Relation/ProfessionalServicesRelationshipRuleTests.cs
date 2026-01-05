// <copyright file="ProfessionalServicesRelationshipTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class ProfessionalServicesRelationshipRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProfessionalServicesRelationshipRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProfessionalDeriveParties()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var relationship = new ProfessionalServicesRelationshipBuilder(this.Transaction).WithProfessionalServicesProvider(internalOrganisation).Build();
            this.Derive();

            var professional = new PersonBuilder(this.Transaction).Build();
            relationship.Professional = professional;
            this.Derive();

            Assert.Contains(professional, relationship.Parties);
        }

        [Fact]
        public void ChangedProfessionalServicesProviderDeriveParties()
        {
            var professional = new PersonBuilder(this.Transaction).Build();
            var relationship = new ProfessionalServicesRelationshipBuilder(this.Transaction).WithProfessional(professional).Build();
            this.Derive();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            relationship.ProfessionalServicesProvider = internalOrganisation;
            this.Derive();

            Assert.Contains(internalOrganisation, relationship.Parties);
        }
    }

    public class ProfessionalServicesRelationshipFromDateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProfessionalServicesRelationshipFromDateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void PeriodActiveThrowValidationError()
        {
            var professional = new PersonBuilder(this.Transaction).WithLastName("professional").Build();
            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(1)).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodActiveThrowValidationError_1()
        {
            var professional = new PersonBuilder(this.Transaction).WithLastName("professional").Build();
            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(-1)).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodActiveThrowValidationError_2()
        {
            var professional = new PersonBuilder(this.Transaction).WithLastName("professional").Build();
            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithThroughDate(this.Transaction.Now().AddDays(1)).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(1)).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PeriodActive)));
        }

        [Fact]
        public void PeriodNotActive()
        {
            var professional = new PersonBuilder(this.Transaction).WithLastName("professional").Build();
            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithThroughDate(this.Transaction.Now().AddDays(1)).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(2)).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void PeriodNotActive_1()
        {
            var professional = new PersonBuilder(this.Transaction).WithLastName("professional").Build();
            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);

            new ProfessionalServicesRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now().AddDays(-2)).WithThroughDate(this.Transaction.Now().AddDays(-1)).WithProfessional(professional).WithProfessionalServicesProvider(this.InternalOrganisation).Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
