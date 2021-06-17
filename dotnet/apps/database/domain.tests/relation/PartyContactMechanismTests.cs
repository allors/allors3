// <copyright file="PartyContactMechanismTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Allors.Database.Domain.TestPopulation;
    using Derivations.Errors;
    using Meta;
    using Xunit;
    using PartyContactMechanism = Domain.PartyContactMechanism;

    public class PartyContactMechanismTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyContactMechanismTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPartyContactMechanism_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var contactMechanism = new TelecommunicationsNumberBuilder(this.Transaction).WithAreaCode("0495").WithContactNumber("493499").WithDescription("cellphone").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new PartyContactMechanismBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithContactMechanism(contactMechanism);
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenPartyContactMechanism_WhenPartyIsDeleted_ThenPartyContactMechanismIsDeleted()
        {
            var contactMechanism = new TelecommunicationsNumberBuilder(this.Transaction).WithAreaCode("0495").WithContactNumber("493499").WithDescription("cellphone").Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(contactMechanism).Build();
            var party = new PersonBuilder(this.Transaction).WithLastName("party").WithPartyContactMechanism(partyContactMechanism).Build();

            this.Transaction.Derive();
            var countBefore = this.Transaction.Extent<PartyContactMechanism>().Count;

            party.Delete();
            this.Transaction.Derive();

            Assert.Equal(countBefore - 1, this.Transaction.Extent<PartyContactMechanism>().Count);
        }
    }

    public class PartyContactMechanismRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyContactMechanismRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedUseAsDefaultThrowValidationError()
        {
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).Build();
            this.Derive();

            partyContactMechanism.UseAsDefault = true;

            var errors = this.Derive().Errors.OfType<DerivationErrorRequired>();
            Assert.Contains(this.M.PartyContactMechanism.ContactPurposes, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedContactPurposesThrowValidationError()
        {
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).WithDefaults().Build())
                .WithUseAsDefault(true)
                .Build();

            {
                var errors = this.Derive().Errors;
                Assert.DoesNotContain(errors, e => e is DerivationErrorRequired);
            }

            partyContactMechanism.RemoveContactPurposes();

            {
                var errors = this.Derive().Errors.OfType<DerivationErrorRequired>();
                Assert.Contains(this.M.PartyContactMechanism.ContactPurposes, errors.SelectMany(v => v.RoleTypes));
            }
        }

        [Fact]
        public void ChangedUseAsDefaultDeriveUseAsDefault()
        {
            var party = new PersonBuilder(this.Transaction).Build();
            var partyContactMechanism1 = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism1);
            this.Derive();

            var partyContactMechanism2 = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism2);
            this.Derive();

            Assert.False(partyContactMechanism1.UseAsDefault);
        }
    }
}
