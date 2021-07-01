// <copyright file="CommunicationTaskRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class CommunicationTaskRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CommunicationTaskRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedCommunicationEventDeriveWorkItem()
        {
            var task = new CommunicationTaskBuilder(this.Transaction).Build();
            this.Derive();

            task.CommunicationEvent = new PhoneCommunicationBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(task.CommunicationEvent, task.WorkItem);
        }

        [Fact]
        public void OnChangedCommunicationEventWorkItemDescriptionDeriveTitle()
        {
            var party = new PersonBuilder(this.Transaction).WithLastName("person").Build();
            this.Derive();

            var commEvent = new PhoneCommunicationBuilder(this.Transaction).WithScheduledStart(this.Transaction.Now()).WithToParty(party).WithSubject("subject").Build();
            this.Derive();

            commEvent.Subject = "changed";
            this.Derive();

            Assert.Equal(commEvent.WorkItemDescription, commEvent.CommunicationTasksWhereCommunicationEvent.First().Title);
        }

        [Fact]
        public void OnCommunicationEventChangedActualEndDeriveCommunicationTaskDateClosed()
        {
            var phoneComEvent = new PhoneCommunicationBuilder(this.Transaction).WithScheduledStart(this.Transaction.Now()).Build();
            this.Derive();

            var task = phoneComEvent.CommunicationTasksWhereCommunicationEvent.ElementAt(0);

            phoneComEvent.ActualEnd = this.Transaction.Now();
            this.Derive();

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
                .WithFromParty(this.InternalOrganisation.CurrentContacts.FirstOrDefault())
                .WithScheduledStart(this.Transaction.Now()).Build();
            this.Derive();

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.FirstOrDefault();

            communicationEvent.ActualEnd = this.Transaction.Now();
            this.Derive();

            Assert.Empty(task.Participants);
        }

        [Fact]
        public void OnChangedCommunicationEventFromPartyDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithScheduledStart(this.Transaction.Now()).Build();
            this.Derive();

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.FirstOrDefault();

            communicationEvent.FromParty = this.InternalOrganisation.CurrentContacts.FirstOrDefault();
            this.Derive();

            Assert.Contains(this.InternalOrganisation.CurrentContacts.FirstOrDefault(), task.Participants);
        }

        [Fact]
        public void OnChangedCommunicationEventToPartyDeriveParticipants()
        {
            var communicationEvent = new PhoneCommunicationBuilder(this.Transaction)
                .WithScheduledStart(this.Transaction.Now()).Build();
            this.Derive();

            var task = communicationEvent.CommunicationTasksWhereCommunicationEvent.FirstOrDefault();

            communicationEvent.ToParty = this.InternalOrganisation.CurrentContacts.FirstOrDefault();
            this.Derive();

            Assert.Contains(this.InternalOrganisation.CurrentContacts.FirstOrDefault(), task.Participants);
        }
    }
}
