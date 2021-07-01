// <copyright file="LetterCorrespondenceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class LetterCorrespondenceTests : DomainTest, IClassFixture<Fixture>
    {
        public LetterCorrespondenceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenLetterCorrespondenceIsBuild_WhenDeriving_ThenStatusIsSet()
        {
            var communication = new LetterCorrespondenceBuilder(this.Transaction)
                .WithSubject("Hello world!")
                .WithOwner(new PersonBuilder(this.Transaction).WithLastName("owner").Build())
                .WithFromParty(new PersonBuilder(this.Transaction).WithLastName("originator").Build())
                .WithToParty(new PersonBuilder(this.Transaction).WithLastName("receiver").Build())
                .Build();

            Assert.False(this.Derive().HasErrors);

            Assert.Equal(communication.CommunicationEventState, new CommunicationEventStates(this.Transaction).Scheduled);
            Assert.Equal(communication.CommunicationEventState, communication.LastCommunicationEventState);
        }

        [Fact]
        public void GivenLetterCorrespondence_WhenDeriving_ThenInvolvedPartiesAreDerived()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").Build();
            var receiver = new PersonBuilder(this.Transaction).WithLastName("receiver").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var communication = new LetterCorrespondenceBuilder(this.Transaction)
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
        public void GivenLetterCorrespondence_WhenOriginatorIsDeleted_ThenCommunicationEventIsDeleted()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").Build();
            var receiver = new PersonBuilder(this.Transaction).WithLastName("receiver").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            new LetterCorrespondenceBuilder(this.Transaction)
                .WithSubject("Hello world!")
                .WithOwner(owner)
                .WithFromParty(originator)
                .WithToParty(receiver)
                .Build();

            this.Transaction.Derive();

            Assert.Single(this.Transaction.Extent<LetterCorrespondence>());

            originator.Delete();
            this.Transaction.Derive();

            Assert.Empty(this.Transaction.Extent<LetterCorrespondence>());
        }
    }

    public class LetterCorrespondenceCommunicationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public LetterCorrespondenceCommunicationRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSubjectDeriveWorkItemDescription()
        {
            var communication = new LetterCorrespondenceBuilder(this.Transaction).Build();
            this.Derive();

            communication.Subject = "subject";
            this.Derive();

            Assert.Contains("subject", communication.WorkItemDescription);
        }

        [Fact]
        public void ChangedToPartyDeriveWorkItemDescription()
        {
            var communication = new LetterCorrespondenceBuilder(this.Transaction).Build();
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

            var communication = new LetterCorrespondenceBuilder(this.Transaction).WithToParty(person).Build();
            this.Derive();

            person.LastName = "changed";
            this.Derive();

            Assert.Contains("changed", communication.WorkItemDescription);
        }
    }

}
