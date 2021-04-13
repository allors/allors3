// <copyright file="CommunicationTaskRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CommunicationTaskRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationTaskRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedCommunicationEventDeriveWorkItem()
        {
            var task = new CommunicationTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            task.CommunicationEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(task.CommunicationEvent, task.WorkItem);
        }

        [Fact]
        public void OnChangedCommunicationEventWorkItemDescriptionDeriveTitle()
        {
            var party = new PersonBuilder(this.Transaction).WithLastName("person").Build();
            this.Transaction.Derive(false);

            var commEvent = new PhoneCommunicationBuilder(this.Transaction).WithScheduledStart(this.Transaction.Now()).WithToParty(party).WithSubject("subject").Build();
            this.Transaction.Derive(false);

            commEvent.Subject = "changed";
            this.Transaction.Derive(false);

            Assert.Equal(commEvent.WorkItemDescription, commEvent.CommunicationTasksWhereCommunicationEvent.First.Title);
        }

        [Fact]
        public void OnCommunicationEventChangedActualEndDeriveCommunicationTaskDateClosed()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var task = phoneComEvent.CommunicationTasksWhereCommunicationEvent[0];

            phoneComEvent.ActualEnd = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.True(task.ExistDateClosed);
        }
    }


    public class CommunicationTaskParticipantsRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationTaskParticipantsRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedCommunicationEventDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithFromParty(this.InternalOrganisation.CurrentContacts.First)
                .WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.First;

            communicationEvent.ActualEnd = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Empty(task.Participants);
        }

        [Fact]
        public void OnChangedCommunicationEventFromPartyDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.First;

            communicationEvent.FromParty = this.InternalOrganisation.CurrentContacts.First;
            this.Transaction.Derive(false);

            Assert.Contains(this.InternalOrganisation.CurrentContacts.First, task.Participants);
        }

        [Fact]
        public void OnChangedCommunicationEventToPartyDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.First;

            communicationEvent.ToParty = this.InternalOrganisation.CurrentContacts.First;
            this.Transaction.Derive(false);

            Assert.Contains(this.InternalOrganisation.CurrentContacts.First, task.Participants);
        }
    }
}
