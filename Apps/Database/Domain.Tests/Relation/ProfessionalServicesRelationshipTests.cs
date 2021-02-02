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

    public class ProfessionalServicesRelationshipDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProfessionalServicesRelationshipDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProfessionalDeriveParties()
        {
            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            var relationship = new ProfessionalServicesRelationshipBuilder(this.Session).WithProfessionalServicesProvider(internalOrganisation).Build();
            this.Session.Derive(false);

            var professional = new PersonBuilder(this.Session).Build();
            relationship.Professional = professional;
            this.Session.Derive(false);

            Assert.Contains(professional, relationship.Parties);
        }

        [Fact]
        public void ChangedProfessionalServicesProviderDeriveParties()
        {
            var professional = new PersonBuilder(this.Session).Build();
            var relationship = new ProfessionalServicesRelationshipBuilder(this.Session).WithProfessional(professional).Build();
            this.Session.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            relationship.ProfessionalServicesProvider = internalOrganisation;
            this.Session.Derive(false);

            Assert.Contains(internalOrganisation, relationship.Parties);
        }
    }
}
