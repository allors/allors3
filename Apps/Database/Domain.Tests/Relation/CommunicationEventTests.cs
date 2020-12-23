// <copyright file="CommunicationEventTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Xunit;

    public class CommunicationEventTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationEventTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCommunicationEvent_WhenInProgress_ThenCurrentObjectStateIsInProgress()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Session)
                .WithOwner(new PersonBuilder(this.Session).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Session).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Session).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Equal(new CommunicationEventStates(this.Session).InProgress, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenCommunicationEvent_WhenInPast_ThenCurrencObjectStateIsCompleted()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Session)
                .WithOwner(new PersonBuilder(this.Session).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Session).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Session).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Session.Now().AddHours(-2))
                .WithActualEnd(this.Session.Now().AddHours(-1))
                .Build();

            this.Session.Derive();

            Assert.Equal(new CommunicationEventStates(this.Session).Completed, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenCommunicationEvent_WhenInFuture_ThenCurrencObjectStateIsScheduled()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Session)
                .WithOwner(new PersonBuilder(this.Session).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Session).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Session).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Session.Now().AddHours(+1))
                .WithActualEnd(this.Session.Now().AddHours(+2))
                .Build();

            this.Session.Derive();

            Assert.Equal(new CommunicationEventStates(this.Session).Scheduled, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenFaceToFaceCommunication_WhenConfirmed_ThenCurrentCommunicationEventStatusMustBeDerived()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Session)
                .WithOwner(new PersonBuilder(this.Session).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Session).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Session).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Equal(new CommunicationEventStates(this.Session).InProgress, communication.CommunicationEventState);

            communication.Close();

            this.Session.Derive();

            Assert.Equal(new CommunicationEventStates(this.Session).Completed, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenCommunication_WhenDerived_ThenCommunicationEventIsAddedToEachParty()
        {
            var owner = new PersonBuilder(this.Session).WithLastName("owner").Build();
            var participant1 = new PersonBuilder(this.Session).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Session).WithLastName("participant2").Build();

            var communication = new FaceToFaceCommunicationBuilder(this.Session)
                .WithOwner(owner)
                .WithFromParty(participant1)
                .WithToParty(participant2)
                .WithSubject("Hello")
                .WithActualStart(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant1, communication.InvolvedParties);
            Assert.Contains(participant2, communication.InvolvedParties);
        }

        [Fact]
        public void GivenCommunicationToOrganisation_WhenDerived_ThenCommunicationEventIsAddedToEachParty()
        {
            var owner = new PersonBuilder(this.Session).WithLastName("owner").Build();
            var participant = new PersonBuilder(this.Session).WithLastName("participant1").Build();
            var organisation = new OrganisationBuilder(this.Session).WithName("organisation").Build();
            var contact = new PersonBuilder(this.Session).WithLastName("participant1").Build();
            new OrganisationContactRelationshipBuilder(this.Session).WithContact(contact).WithOrganisation(organisation).Build();

            var communication = new FaceToFaceCommunicationBuilder(this.Session)
                .WithOwner(owner)
                .WithFromParty(participant)
                .WithToParty(contact)
                .WithSubject("Hello")
                .WithActualStart(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Equal(4, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant, communication.InvolvedParties);
            Assert.Contains(contact, communication.InvolvedParties);
            Assert.Contains(organisation, communication.InvolvedParties);
        }

        [Fact]
        public void GivenCommunication_WhenPartiesChange_ThenPartiesAreUpdated()
        {
            var owner = new PersonBuilder(this.Session).WithLastName("owner").Build();
            var participant1 = new PersonBuilder(this.Session).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Session).WithLastName("participant1").Build();
            var organisation = new OrganisationBuilder(this.Session).WithName("organisation").Build();
            var contact = new PersonBuilder(this.Session).WithLastName("participant1").Build();
            new OrganisationContactRelationshipBuilder(this.Session).WithContact(contact).WithOrganisation(organisation).Build();

            var communication = new FaceToFaceCommunicationBuilder(this.Session)
                .WithOwner(owner)
                .WithFromParty(participant1)
                .WithToParty(contact)
                .WithSubject("Hello")
                .WithActualStart(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.Equal(4, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant1, communication.InvolvedParties);
            Assert.Contains(contact, communication.InvolvedParties);
            Assert.Contains(organisation, communication.InvolvedParties);

            communication.ToParty = participant2;

            this.Session.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant1, communication.InvolvedParties);
            Assert.Contains(participant2, communication.InvolvedParties);
        }
    }
    public class CommunicationEventDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationEventDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedOwnerDeriveOwner()
        {
            var employee = new Employments(this.Session).Extent().Select(v => v.Employee).First();
            this.Session.SetUser(employee);

            var phoneComEvent = new PhoneCommunicationBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.Equal(employee, phoneComEvent.Owner);
        }

        [Fact]
        public void OnChangedOwnerDeriveSecurityToken()
        {
            var employee = new Employments(this.Session).Extent().Select(v => v.Employee).First();
            this.Session.SetUser(employee);

            var phoneComEvent = new PhoneCommunicationBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.Contains(employee.OwnerSecurityToken, phoneComEvent.SecurityTokens);
        }

        [Fact]
        public void OnChangedScheduledEndThrowDerivationError()
        {
            var employee = new Employments(this.Session).Extent().Select(v => v.Employee).First();
            this.Session.SetUser(employee);

            var phoneComEvent = new PhoneCommunicationBuilder(this.Session).WithScheduledStart(this.Session.Now()).Build();

            this.Session.Derive(false);

            phoneComEvent.ScheduledEnd = this.Session.Now().AddHours(-1);

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("Scheduled end date before scheduled start date")));
        }

        [Fact]
        public void OnChangedScheduledEndScheduledStartThrowDerivationError()
        {
            var employee = new Employments(this.Session).Extent().Select(v => v.Employee).First();
            this.Session.SetUser(employee);

            var phoneComEvent = new PhoneCommunicationBuilder(this.Session).WithScheduledStart(this.Session.Now()).WithScheduledEnd(this.Session.Now().AddHours(-1)).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("Scheduled end date before scheduled start date"));
        }

    }
}
