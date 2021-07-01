// <copyright file="WebSiteCommunicationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class WebSiteCommunicationTests : DomainTest, IClassFixture<Fixture>
    {
        public WebSiteCommunicationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWebSiteCommunication_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var person = new PersonBuilder(this.Transaction).WithLastName("person").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new WebSiteCommunicationBuilder(this.Transaction).WithFromParty(person).WithToParty(person);
            var communication = builder.Build();

            var validation = this.Derive();

            Assert.True(validation.HasErrors);

            this.Transaction.Rollback();

            builder.WithSubject("Website communication");
            communication = builder.Build();

            validation = this.Derive();

            Assert.False(validation.HasErrors);

            Assert.Equal(communication.CommunicationEventState, new CommunicationEventStates(this.Transaction).Scheduled);
            Assert.Equal(communication.CommunicationEventState, communication.LastCommunicationEventState);
        }

        [Fact]
        public void GivenWebSiteCommunication_WhenDeriving_ThenInvolvedPartiesAreDerived()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").Build();
            var receiver = new PersonBuilder(this.Transaction).WithLastName("receiver").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var communication = new WebSiteCommunicationBuilder(this.Transaction)
                .WithSubject("Hello world!")
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
        public void GivenWebSiteCommunication_WhenOriginatorIsDeleted_ThenCommunicationEventIsDeleted()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").Build();
            var receiver = new PersonBuilder(this.Transaction).WithLastName("receiver").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            new WebSiteCommunicationBuilder(this.Transaction)
                .WithSubject("Hello world!")
                .WithOwner(owner)
                .WithFromParty(originator)
                .WithToParty(receiver)
                .Build();

            this.Transaction.Derive();

            Assert.Single(this.Transaction.Extent<WebSiteCommunication>());

            originator.Delete();
            this.Transaction.Derive();

            Assert.Empty(this.Transaction.Extent<WebSiteCommunication>());
        }
    }

    public class WebSiteCommunicationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WebSiteCommunicationRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSubjectDeriveWorkItemDescription()
        {
            var communication = new WebSiteCommunicationBuilder(this.Transaction).Build();
            this.Derive();

            communication.Subject = "subject";
            this.Derive();

            Assert.Contains("subject", communication.WorkItemDescription);
        }

        [Fact]
        public void ChangedToPartyDeriveWorkItemDescription()
        {
            var communication = new WebSiteCommunicationBuilder(this.Transaction).Build();
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

            var communication = new WebSiteCommunicationBuilder(this.Transaction).WithToParty(person).Build();
            this.Derive();

            person.LastName = "changed";
            this.Derive();

            Assert.Contains("changed", communication.WorkItemDescription);
        }
    }
}
