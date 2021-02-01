// <copyright file="CommunicationTaskDerivationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CommunicationTaskDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationTaskDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedCommunicationEventDeriveWorkItem()
        {
            var task = new CommunicationTaskBuilder(this.Session).Build();
            this.Session.Derive(false);

            task.CommunicationEvent = new PhoneCommunicationBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(task.CommunicationEvent, task.WorkItem);
        }

        [Fact]
        public void OnChangedCommunicationEventWorkItemDescriptionDeriveTitle()
        {
            var party = new PersonBuilder(this.Session).WithLastName("person").Build();
            this.Session.Derive(false);

            var commEvent = new PhoneCommunicationBuilder(this.Session).WithScheduledStart(this.Session.Now()).WithToParty(party).WithSubject("subject").Build();
            this.Session.Derive(false);

            commEvent.Subject = "changed";
            this.Session.Derive(false);

            Assert.Equal(commEvent.WorkItemDescription, commEvent.CommunicationTasksWhereCommunicationEvent.First.Title);
        }

        [Fact]
        public void OnCommunicationEventChangedActualEndDeriveCommunicationTaskDateClosed()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Session).WithScheduledStart(this.Session.Now()).Build();
            this.Session.Derive(false);

            var task = phoneComEvent.CommunicationTasksWhereCommunicationEvent[0];

            phoneComEvent.ActualEnd = this.Session.Now();
            this.Session.Derive(false);

            Assert.True(task.ExistDateClosed);
        }
    }


    public class CommunicationTaskParticipantsDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationTaskParticipantsDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedCommunicationEventDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Session)
                .WithFromParty(this.InternalOrganisation.CurrentContacts.First)
                .WithScheduledStart(this.Session.Now()).Build();
            this.Session.Derive(false);

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.First;

            communicationEvent.ActualEnd = this.Session.Now();
            this.Session.Derive(false);

            Assert.Empty(task.Participants);
        }

        [Fact]
        public void OnChangedCommunicationEventFromPartyDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Session)
                .WithScheduledStart(this.Session.Now()).Build();
            this.Session.Derive(false);

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.First;

            communicationEvent.FromParty = this.InternalOrganisation.CurrentContacts.First;
            this.Session.Derive(false);

            Assert.Contains(this.InternalOrganisation.CurrentContacts.First, task.Participants);
        }

        [Fact]
        public void OnChangedCommunicationEventToPartyDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Session)
                .WithScheduledStart(this.Session.Now()).Build();
            this.Session.Derive(false);

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.First;

            communicationEvent.ToParty = this.InternalOrganisation.CurrentContacts.First;
            this.Session.Derive(false);

            Assert.Contains(this.InternalOrganisation.CurrentContacts.First, task.Participants);
        }
    }
}
