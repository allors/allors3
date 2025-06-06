// <copyright file="FaxCommunicationTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class FaxCommunicationTests : DomainTest, IClassFixture<Fixture>
    {
        public FaxCommunicationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenFaxCommunicationIsBuild_WhenDeriving_ThenStatusIsSet()
        {
            var communication = new FaxCommunicationBuilder(this.Transaction)
                .WithSubject("subject")
                .WithOwner(new PersonBuilder(this.Transaction).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Transaction).WithLastName("originator").Build())
                .WithToParty(new PersonBuilder(this.Transaction).WithLastName("receiver").Build())
                .Build();

            Assert.False(this.Derive().HasErrors);

            Assert.Equal(communication.CommunicationEventState, new CommunicationEventStates(this.Transaction).Scheduled);
            Assert.Equal(communication.CommunicationEventState, communication.LastCommunicationEventState);
        }

        [Fact]
        public void GivenFaxCommunication_WhenDeriving_ThenInvolvedPartiesAreDerived()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").Build();
            var receiver = new PersonBuilder(this.Transaction).WithLastName("receiver").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var communication = new FaxCommunicationBuilder(this.Transaction)
                .WithSubject("subject")
                .WithOwner(owner)
                .WithFromParty(originator)
                .WithToParty(receiver)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count());
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(originator, communication.InvolvedParties);
            Assert.Contains(receiver, communication.InvolvedParties);
        }

        [Fact]
        public void GivenFaxCommunication_WhenOriginatorIsDeleted_ThenCommunicationEventIsDeleted()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").Build();
            var receiver = new PersonBuilder(this.Transaction).WithLastName("receiver").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            new FaxCommunicationBuilder(this.Transaction)
                .WithSubject("Hello world!")
                .WithOwner(owner)
                .WithFromParty(originator)
                .WithToParty(receiver)
                .Build();

            this.Transaction.Derive();

            Assert.Single(this.Transaction.Extent<FaxCommunication>());

            originator.Delete();
            this.Transaction.Derive();

            Assert.Empty(this.Transaction.Extent<FaxCommunication>());
        }
    }


    public class FaxCommunicationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public FaxCommunicationRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSubjectDeriveWorkItemDescription()
        {
            var communication = new FaxCommunicationBuilder(this.Transaction).Build();
            this.Derive();

            communication.Subject = "subject";
            this.Derive();

            Assert.Contains("subject", communication.WorkItemDescription);
        }

        [Fact]
        public void ChangedToPartyDeriveWorkItemDescription()
        {
            var communication = new FaxCommunicationBuilder(this.Transaction).Build();
            this.Derive();

            var person = new PersonBuilder(this.Transaction).WithLastName("person").Build();
            this.Derive();

            communication.ToParty = person;
            this.Derive();

            Assert.Contains("person", communication.WorkItemDescription);
        }

        [Fact]
        public void ChangedPartyPartyNameDeriveWorkItemDescription()
        {
            var person = new PersonBuilder(this.Transaction).WithLastName("person").Build();
            this.Derive();

            var communication = new FaxCommunicationBuilder(this.Transaction).WithToParty(person).Build();
            this.Derive();

            person.LastName = "changed";
            this.Derive();

            Assert.Contains("changed", communication.WorkItemDescription);
        }
    }
}
