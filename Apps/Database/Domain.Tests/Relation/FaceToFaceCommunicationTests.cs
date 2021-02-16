// <copyright file="FaceToFaceCommunicationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class FaceToFaceCommunicationTests : DomainTest, IClassFixture<Fixture>
    {
        public FaceToFaceCommunicationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenFaceToFaceCommunicationIsBuild_WhenDeriving_ThenStatusIsSet()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(new PersonBuilder(this.Transaction).WithLastName("owner").Build())
                .WithSubject("subject")
                .WithFromParty(new PersonBuilder(this.Transaction).WithLastName("participant1").Build())
                .WithToParty(new PersonBuilder(this.Transaction).WithLastName("participant2").Build())
                .WithActualStart(this.Transaction.Now())
                .Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            Assert.Equal(communication.CommunicationEventState, new CommunicationEventStates(this.Transaction).InProgress);
            Assert.Equal(communication.CommunicationEventState, communication.LastCommunicationEventState);
        }

        [Fact]
        public void GivenFaceToFaceCommunication_WhenDeriving_ThenInvolvedPartiesAreDerived()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var participant1 = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Transaction).WithLastName("participant2").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(owner)
                .WithSubject("subject")
                .WithFromParty(participant1)
                .WithToParty(participant2)
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(participant1, communication.InvolvedParties);
            Assert.Contains(participant2, communication.InvolvedParties);
            Assert.Contains(owner, communication.InvolvedParties);
        }

        [Fact]
        public void GivenFaceToFaceCommunication_WhenParticipantIsDeleted_ThenCommunicationEventIsDeleted()
        {
            var participant1 = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Transaction).WithLastName("participant2").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithSubject("subject")
                .WithFromParty(participant1)
                .WithToParty(participant2)
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Single(this.Transaction.Extent<FaceToFaceCommunication>());

            participant2.Delete();
            this.Transaction.Derive();

            Assert.Empty(this.Transaction.Extent<FaceToFaceCommunication>());
        }
    }

    public class FaceToFaceCommunicationDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public FaceToFaceCommunicationDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSubjectDeriveWorkItemDescription()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            communication.Subject = "subject";
            this.Transaction.Derive(false);

            Assert.Contains("subject", communication.WorkItemDescription);
        }

        [Fact]
        public void ChangedToPartyDeriveWorkItemDescription()
        {
            var communication = new FaceToFaceCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var person = new PersonBuilder(this.Transaction).WithLastName("person").Build();
            this.Transaction.Derive(false);

            communication.ToParty = person;
            this.Transaction.Derive(false);

            Assert.Contains("person", communication.WorkItemDescription);
        }

        [Fact]
        public void ChangedPartyPartyNameDeriveWorkItemDescription()
        {
            var person = new PersonBuilder(this.Transaction).WithLastName("person").Build();
            this.Transaction.Derive(false);

            var communication = new FaceToFaceCommunicationBuilder(this.Transaction).WithToParty(person).Build();
            this.Transaction.Derive(false);

            person.LastName = "changed";
            this.Transaction.Derive(false);

            Assert.Contains("changed", communication.WorkItemDescription);
        }
    }

    [Trait("Category", "Security")]
    public class FaceToFaceCommunicationSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public FaceToFaceCommunicationSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenCurrentUserIsUnknown_WhenAccessingFaceToFaceCommunicationWithOwner_ThenOwnerSecurityTokenIsApplied()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var participant1 = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Transaction).WithLastName("participant2").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(owner)
                .WithSubject("subject")
                .WithFromParty(participant1)
                .WithToParty(participant2)
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, communication.SecurityTokens.Count);
            Assert.Contains(new SecurityTokens(this.Transaction).DefaultSecurityToken, communication.SecurityTokens);
            Assert.Contains(owner.OwnerSecurityToken, communication.SecurityTokens);
        }

        [Fact]
        public void GivenCurrentUserIsKnown_WhenAccessingFaceToFaceCommunicationWithOwner_ThenOwnerSecurityTokenIsApplied()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();
            var participant1 = new PersonBuilder(this.Transaction).WithLastName("participant1").Build();
            var participant2 = new PersonBuilder(this.Transaction).WithLastName("participant2").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(owner);

            var communication = new FaceToFaceCommunicationBuilder(this.Transaction)
                .WithOwner(owner)
                .WithSubject("subject")
                .WithFromParty(participant1)
                .WithToParty(participant2)
                .WithActualStart(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, communication.SecurityTokens.Count);
            Assert.Contains(new SecurityTokens(this.Transaction).DefaultSecurityToken, communication.SecurityTokens);
            Assert.Contains(owner.OwnerSecurityToken, communication.SecurityTokens);
        }
    }
}
