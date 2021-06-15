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
            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(new PersonBuilder(this.Transaction).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Transaction).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Transaction).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new CommunicationEventStates(this.Transaction).InProgress, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenCommunicationEvent_WhenInPast_ThenCurrencObjectStateIsCompleted()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(new PersonBuilder(this.Transaction).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Transaction).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Transaction).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Transaction.Now().AddHours(-2))
                .WithActualEnd(this.Transaction.Now().AddHours(-1))
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenCommunicationEvent_WhenInFuture_ThenCurrencObjectStateIsScheduled()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(new PersonBuilder(this.Transaction).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Transaction).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Transaction).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Transaction.Now().AddHours(+1))
                .WithActualEnd(this.Transaction.Now().AddHours(+2))
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new CommunicationEventStates(this.Transaction).Scheduled, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenFaceToFaceCommunication_WhenConfirmed_ThenCurrentCommunicationEventStatusMustBeDerived()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(new PersonBuilder(this.Transaction).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Transaction).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Transaction).WithLastName("participant2").Build())
                .WithSubject("Hello")
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new CommunicationEventStates(this.Transaction).InProgress, communication.CommunicationEventState);

            communication.Close();

            this.Transaction.Derive();

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, communication.CommunicationEventState);
        }

        [Fact]
        public void GivenCommunication_WhenDerived_ThenCommunicationEventIsAddedToEachParty()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var participant1 = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Transaction).WithLastName("participant2").Build();

            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(owner)
                .WithFromParty(participant1)
                .WithToParty(participant2)
                .WithSubject("Hello")
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant1, communication.InvolvedParties);
            Assert.Contains(participant2, communication.InvolvedParties);
        }

        [Fact]
        public void GivenCommunicationToOrganisation_WhenDerived_ThenCommunicationEventIsAddedToEachParty()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var participant = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            var contact = new PersonBuilder(this.Transaction).WithLastName("contact").Build();
            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(organisation).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();

            this.Transaction.Derive();

            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(owner)
                .WithFromParty(participant)
                .WithToParty(contact)
                .WithSubject("Hello")
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(4, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant, communication.InvolvedParties);
            Assert.Contains(contact, communication.InvolvedParties);
            Assert.Contains(organisation, communication.InvolvedParties);
        }

        [Fact]
        public void GivenCommunication_WhenPartiesChange_ThenPartiesAreUpdated()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var participant1 = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            var contact = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(organisation).Build();

            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(owner)
                .WithFromParty(participant1)
                .WithToParty(contact)
                .WithSubject("Hello")
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(4, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant1, communication.InvolvedParties);
            Assert.Contains(contact, communication.InvolvedParties);
            Assert.Contains(organisation, communication.InvolvedParties);

            communication.ToParty = participant2;

            this.Transaction.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(participant1, communication.InvolvedParties);
            Assert.Contains(participant2, communication.InvolvedParties);
        }
    }

    public class CommunicationEventOnInitTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationEventOnInitTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedOwnerDeriveOwner()
        {
            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.Equal(employee, phoneComEvent.Owner);
        }
    }

    public class CommunicationEventRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationEventRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedOwnerDeriveSecurityToken()
        {
            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(employee.OwnerSecurityToken, phoneComEvent.SecurityTokens);
        }

        [Fact]
        public void OnChangedScheduledEndThrowDerivationError()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ScheduledEnd = this.Transaction.Now().AddHours(-1);

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("Scheduled end date before scheduled start date"));
        }

        [Fact]
        public void OnChangedActualEndActualStartThrowDerivationError()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithActualStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ActualEnd = this.Transaction.Now().AddHours(-1);

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("Actual end date before actual start date"));
        }

        [Fact]
        public void OnChangedActualStartDeriveCommunicationEventStateScheduled()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ActualStart = this.Transaction.Now().AddHours(1);
            this.Transaction.Derive(false);

            Assert.Equal(new CommunicationEventStates(this.Transaction).Scheduled, phoneComEvent.CommunicationEventState);
        }

        [Fact]
        public void OnChangedActualStartDeriveCommunicationEventStateInProgress()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ActualStart = this.Transaction.Now().AddHours(-1);
            this.Transaction.Derive(false);

            Assert.Equal(new CommunicationEventStates(this.Transaction).InProgress, phoneComEvent.CommunicationEventState);
        }

        [Fact]
        public void OnChangedActualEndDeriveCommunicationEventStateInProgress()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithActualStart(this.Transaction.Now().AddHours(-2))
                .WithActualEnd(this.Transaction.Now().AddHours(-1))
                .Build();
            this.Transaction.Derive(false);

            phoneComEvent.ActualEnd = this.Transaction.Now().AddHours(1);
            this.Transaction.Derive(false);

            Assert.Equal(new CommunicationEventStates(this.Transaction).InProgress, phoneComEvent.CommunicationEventState);
        }

        [Fact]
        public void OnChangedActualEndDeriveCommunicationEventStateCompleted()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithActualStart(this.Transaction.Now().AddHours(-2)).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ActualEnd = this.Transaction.Now().AddHours(-1);
            this.Transaction.Derive(false);

            Assert.Equal(new CommunicationEventStates(this.Transaction).Completed, phoneComEvent.CommunicationEventState);
        }

        [Fact]
        public void OnChangedScheduledStartDeriveInitialScheduledStart()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ScheduledStart = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Equal(phoneComEvent.ScheduledStart, phoneComEvent.InitialScheduledStart);
        }

        [Fact]
        public void OnChangedInitialScheduledStartDeriveInitialScheduledStart()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithInitialScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ScheduledStart = this.Transaction.Now().AddDays(1);
            this.Transaction.Derive(false);

            phoneComEvent.RemoveInitialScheduledStart();
            this.Transaction.Derive(false);

            Assert.Equal(phoneComEvent.ScheduledStart, phoneComEvent.InitialScheduledStart);
        }

        [Fact]
        public void OnChangedScheduledEndDeriveInitialScheduledEnd()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ScheduledEnd = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Equal(phoneComEvent.ScheduledEnd, phoneComEvent.InitialScheduledEnd);
        }

        [Fact]
        public void OnChangedInitialScheduledEndDeriveInitialScheduledEnd()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithInitialScheduledEnd(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            phoneComEvent.ScheduledEnd = this.Transaction.Now().AddDays(1);
            this.Transaction.Derive(false);

            phoneComEvent.RemoveInitialScheduledEnd();
            this.Transaction.Derive(false);

            Assert.Equal(phoneComEvent.ScheduledEnd, phoneComEvent.InitialScheduledEnd);
        }

        [Fact]
        public void OnChangedDeriveCommunicationTask()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            Assert.Single(phoneComEvent.CommunicationTasksWhereCommunicationEvent);
        }

        [Fact]
        public void OnChangedFromPartyDeriveInvolvedParties()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var party = new PersonBuilder(this.Transaction).Build();
            phoneComEvent.FromParty = party;
            this.Transaction.Derive(false);

            Assert.Contains(party, phoneComEvent.InvolvedParties);
        }

        [Fact]
        public void OnChangedToPartyDeriveInvolvedParties()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var party = new PersonBuilder(this.Transaction).Build();
            phoneComEvent.ToParty = party;
            this.Transaction.Derive(false);

            Assert.Contains(party, phoneComEvent.InvolvedParties);
        }

        [Fact]
        public void OnChangedOwnerDeriveInvolvedParties()
        {
            var employee = new Employments(this.Transaction).Extent().Select(v => v.Employee).First();
            this.Transaction.SetUser(employee);

            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(employee, phoneComEvent.InvolvedParties);
        }
    }
}
