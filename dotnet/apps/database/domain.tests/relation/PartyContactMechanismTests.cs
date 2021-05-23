// <copyright file="PartyContactMechanismTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Xunit;

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

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithContactMechanism(contactMechanism);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
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
            this.Transaction.Derive(false);

            partyContactMechanism.UseAsDefault = true;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: PartyContactMechanism.ContactPurposes"));
        }

        [Fact]
        public void ChangedContactPurposesThrowValidationError()
        {
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice).WithUseAsDefault(true).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Equals("AssertExists: PartyContactMechanism.ContactPurposes"));

            partyContactMechanism.RemoveContactPurposes();

            errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: PartyContactMechanism.ContactPurposes"));
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
            this.Transaction.Derive(false);

            var partyContactMechanism2 = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism2);
            this.Transaction.Derive(false);

            Assert.False(partyContactMechanism1.UseAsDefault);
        }
    }
}
