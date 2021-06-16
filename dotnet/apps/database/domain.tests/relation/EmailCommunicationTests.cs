// <copyright file="EmailCommunicationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class EmailCommunicationTests : DomainTest, IClassFixture<Fixture>
    {
        public EmailCommunicationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenEmailCommunicationIsBuild_WhenDeriving_ThenStatusIsSet()
        {
            var personalEmailAddress = new ContactMechanismPurposes(this.Transaction).PersonalEmailAddress;

            var originatorEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("originator@allors.com").Build();
            var originatorContact = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(originatorEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").WithPartyContactMechanism(originatorContact).Build();

            var addresseeEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("addressee@allors.com").Build();
            var addresseeContact = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(addresseeEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var addressee = new PersonBuilder(this.Transaction).WithLastName("addressee").WithPartyContactMechanism(addresseeContact).Build();

            var communication = new EmailCommunicationBuilder(this.Transaction)
                .WithOwner(this.Administrator)
                .WithSubject("Hello")
                .WithDescription("Hello world!")
                .WithFromParty(originator)
                .WithToParty(addressee)
                .WithFromEmail(originatorEmail)
                .WithToEmail(addresseeEmail)
                .Build();

            Assert.False(this.Derive().HasErrors);

            Assert.Equal(communication.CommunicationEventState, new CommunicationEventStates(this.Transaction).Scheduled);
            Assert.Equal(communication.CommunicationEventState, communication.LastCommunicationEventState);
        }

        [Fact]
        public void GivenEmailCommunication_WhenDeriving_ThenInvolvedPartiesAreDerived()
        {
            var owner = new PersonBuilder(this.Transaction).WithLastName("owner").Build();

            var personalEmailAddress = new ContactMechanismPurposes(this.Transaction).PersonalEmailAddress;

            var originatorEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("originator@allors.com").Build();
            var originatorContact = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(originatorEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").WithPartyContactMechanism(originatorContact).Build();

            var addresseeEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("addressee@allors.com").Build();
            var addresseeContact = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(addresseeEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var addressee = new PersonBuilder(this.Transaction).WithLastName("addressee").WithPartyContactMechanism(addresseeContact).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var communication = new EmailCommunicationBuilder(this.Transaction)
                .WithSubject("Hello")
                .WithDescription("Hello world!")
                .WithOwner(owner)
                .WithFromParty(originator)
                .WithToParty(addressee)
                .WithFromEmail(originatorEmail)
                .WithToEmail(addresseeEmail)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(originator, communication.InvolvedParties);
            Assert.Contains(addressee, communication.InvolvedParties);
        }

        [Fact]
        public void GivenEmailCommunication_WhenOriginatorIsDeleted_ThenCommunicationEventIsDeleted()
        {
            var personalEmailAddress = new ContactMechanismPurposes(this.Transaction).PersonalEmailAddress;

            var originatorEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("originator@allors.com").Build();
            var originatorContact = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(originatorEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var originator = new PersonBuilder(this.Transaction).WithLastName("originator").WithPartyContactMechanism(originatorContact).Build();

            var addresseeEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("addressee@allors.com").Build();
            var addresseeContact = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(addresseeEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var addressee = new PersonBuilder(this.Transaction).WithLastName("addressee").WithPartyContactMechanism(addresseeContact).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var communication = new EmailCommunicationBuilder(this.Transaction)
                .WithSubject("Hello")
                .WithDescription("Hello world!")
                .WithFromParty(originator)
                .WithToParty(addressee)
                .WithFromEmail(originatorEmail)
                .WithToEmail(addresseeEmail)
                .Build();

            this.Transaction.Derive();

            Assert.Single(this.Transaction.Extent<EmailCommunication>());

            originator.Delete();
            this.Transaction.Derive();

            Assert.Empty(this.Transaction.Extent<EmailCommunication>());
        }
    }


    public class EmailCommunicationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public EmailCommunicationRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSubjectDeriveSubject()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Transaction)
                .WithEmailTemplate(new EmailTemplateBuilder(this.Transaction).WithSubjectTemplate("subjectfromtemplate").Build())
                .WithSubject("subject")
                .Build();
            this.Derive();

            emailCommunication.RemoveSubject();
            this.Derive();

            Assert.Equal("subjectfromtemplate", emailCommunication.Subject);
        }

        [Fact]
        public void ChangedEmailTemplateDeriveSubject()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Transaction)
                .Build();
            this.Derive();

            emailCommunication.EmailTemplate = new EmailTemplateBuilder(this.Transaction).WithSubjectTemplate("subjectfromtemplate").Build();
            this.Derive();

            Assert.Equal("subjectfromtemplate", emailCommunication.Subject);
        }

        [Fact]
        public void ChangedSubjectDeriveWorkItemDescription()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Transaction)
                .Build();
            this.Derive();

            emailCommunication.Subject = "subject";
            this.Derive();

            Assert.Contains("subject", emailCommunication.WorkItemDescription);
        }

        [Fact]
        public void ChangedToEmailDeriveWorkItemDescription()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Transaction)
                .Build();
            this.Derive();

            var to = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("email@something.com").Build();
            emailCommunication.ToEmail = to;
            this.Derive();

            Assert.Contains("email@something.com", emailCommunication.WorkItemDescription);
        }
    }
}
