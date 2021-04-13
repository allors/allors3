// <copyright file="ProfessionalServicesRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class ProfessionalServicesRelationshipRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProfessionalServicesRelationshipRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProfessionalDeriveParties()
        {
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var relationship = new ProfessionalServicesRelationshipBuilder(this.Transaction).WithProfessionalServicesProvider(internalOrganisation).Build();
            this.Transaction.Derive(false);

            var professional = new PersonBuilder(this.Transaction).Build();
            relationship.Professional = professional;
            this.Transaction.Derive(false);

            Assert.Contains(professional, relationship.Parties);
        }

        [Fact]
        public void ChangedProfessionalServicesProviderDeriveParties()
        {
            var professional = new PersonBuilder(this.Transaction).Build();
            var relationship = new ProfessionalServicesRelationshipBuilder(this.Transaction).WithProfessional(professional).Build();
            this.Transaction.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            relationship.ProfessionalServicesProvider = internalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(internalOrganisation, relationship.Parties);
        }
    }
}
