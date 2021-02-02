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
            var contactMechanism = new TelecommunicationsNumberBuilder(this.Session).WithAreaCode("0495").WithContactNumber("493499").WithDescription("cellphone").Build();
            this.Session.Derive();
            this.Session.Commit();

            var builder = new PartyContactMechanismBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithContactMechanism(contactMechanism);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPartyContactMechanism_WhenPartyIsDeleted_ThenPartyContactMechanismIsDeleted()
        {
            var contactMechanism = new TelecommunicationsNumberBuilder(this.Session).WithAreaCode("0495").WithContactNumber("493499").WithDescription("cellphone").Build();
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(contactMechanism).Build();
            var party = new PersonBuilder(this.Session).WithLastName("party").WithPartyContactMechanism(partyContactMechanism).Build();

            this.Session.Derive();
            var countBefore = this.Session.Extent<PartyContactMechanism>().Count;

            party.Delete();
            this.Session.Derive();

            Assert.Equal(countBefore - 1, this.Session.Extent<PartyContactMechanism>().Count);
        }
    }

    public class PartyContactMechanismDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartyContactMechanismDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedUseAsDefaultThrowValidationError()
        {
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session).Build();
            this.Session.Derive(false);

            partyContactMechanism.UseAsDefault = true;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: PartyContactMechanism.ContactPurposes"));
        }

        [Fact]
        public void ChangedContactPurposesThrowValidationError()
        {
            var partyContactMechanism = new PartyContactMechanismBuilder(this.Session).WithContactPurpose(new ContactMechanismPurposes(this.Session).SalesOffice).WithUseAsDefault(true).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Equals("AssertExists: PartyContactMechanism.ContactPurposes"));

            partyContactMechanism.RemoveContactPurposes();

            errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExists: PartyContactMechanism.ContactPurposes"));
        }

        [Fact]
        public void ChangedUseAsDefaultDeriveUseAsDefault()
        {
            var party = new PersonBuilder(this.Session).Build();
            var partyContactMechanism1 = new PartyContactMechanismBuilder(this.Session)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Session).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism1);
            this.Session.Derive(false);

            var partyContactMechanism2 = new PartyContactMechanismBuilder(this.Session)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).SalesOffice)
                .WithContactMechanism(new EmailAddressBuilder(this.Session).Build())
                .WithUseAsDefault(true)
                .Build();
            party.AddPartyContactMechanism(partyContactMechanism2);
            this.Session.Derive(false);

            Assert.False(partyContactMechanism1.UseAsDefault);
        }
    }
}
