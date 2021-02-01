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
            var personalEmailAddress = new ContactMechanismPurposes(this.Session).PersonalEmailAddress;

            var originatorEmail = new EmailAddressBuilder(this.Session).WithElectronicAddressString("originator@allors.com").Build();
            var originatorContact = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(originatorEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var originator = new PersonBuilder(this.Session).WithLastName("originator").WithPartyContactMechanism(originatorContact).Build();

            var addresseeEmail = new EmailAddressBuilder(this.Session).WithElectronicAddressString("addressee@allors.com").Build();
            var addresseeContact = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(addresseeEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var addressee = new PersonBuilder(this.Session).WithLastName("addressee").WithPartyContactMechanism(addresseeContact).Build();

            var communication = new EmailCommunicationBuilder(this.Session)
                .WithOwner(this.Administrator)
                .WithSubject("Hello")
                .WithDescription("Hello world!")
                .WithFromParty(originator)
                .WithToParty(addressee)
                .WithFromEmail(originatorEmail)
                .WithToEmail(addresseeEmail)
                .Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            Assert.Equal(communication.CommunicationEventState, new CommunicationEventStates(this.Session).Scheduled);
            Assert.Equal(communication.CommunicationEventState, communication.LastCommunicationEventState);
        }

        [Fact]
        public void GivenEmailCommunication_WhenDeriving_ThenInvolvedPartiesAreDerived()
        {
            var owner = new PersonBuilder(this.Session).WithLastName("owner").Build();

            var personalEmailAddress = new ContactMechanismPurposes(this.Session).PersonalEmailAddress;

            var originatorEmail = new EmailAddressBuilder(this.Session).WithElectronicAddressString("originator@allors.com").Build();
            var originatorContact = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(originatorEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var originator = new PersonBuilder(this.Session).WithLastName("originator").WithPartyContactMechanism(originatorContact).Build();

            var addresseeEmail = new EmailAddressBuilder(this.Session).WithElectronicAddressString("addressee@allors.com").Build();
            var addresseeContact = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(addresseeEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var addressee = new PersonBuilder(this.Session).WithLastName("addressee").WithPartyContactMechanism(addresseeContact).Build();

            this.Session.Derive();
            this.Session.Commit();

            var communication = new EmailCommunicationBuilder(this.Session)
                .WithSubject("Hello")
                .WithDescription("Hello world!")
                .WithOwner(owner)
                .WithFromParty(originator)
                .WithToParty(addressee)
                .WithFromEmail(originatorEmail)
                .WithToEmail(addresseeEmail)
                .Build();

            this.Session.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(originator, communication.InvolvedParties);
            Assert.Contains(addressee, communication.InvolvedParties);
        }

        [Fact]
        public void GivenEmailCommunication_WhenOriginatorIsDeleted_ThenCommunicationEventIsDeleted()
        {
            var personalEmailAddress = new ContactMechanismPurposes(this.Session).PersonalEmailAddress;

            var originatorEmail = new EmailAddressBuilder(this.Session).WithElectronicAddressString("originator@allors.com").Build();
            var originatorContact = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(originatorEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var originator = new PersonBuilder(this.Session).WithLastName("originator").WithPartyContactMechanism(originatorContact).Build();

            var addresseeEmail = new EmailAddressBuilder(this.Session).WithElectronicAddressString("addressee@allors.com").Build();
            var addresseeContact = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(addresseeEmail).WithContactPurpose(personalEmailAddress).WithUseAsDefault(true).Build();
            var addressee = new PersonBuilder(this.Session).WithLastName("addressee").WithPartyContactMechanism(addresseeContact).Build();

            this.Session.Derive();
            this.Session.Commit();

            var communication = new EmailCommunicationBuilder(this.Session)
                .WithSubject("Hello")
                .WithDescription("Hello world!")
                .WithFromParty(originator)
                .WithToParty(addressee)
                .WithFromEmail(originatorEmail)
                .WithToEmail(addresseeEmail)
                .Build();

            this.Session.Derive();

            Assert.Single(this.Session.Extent<EmailCommunication>());

            originator.Delete();
            this.Session.Derive();

            Assert.Empty(this.Session.Extent<EmailCommunication>());
        }
    }


    public class EmailCommunicationDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public EmailCommunicationDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSubjectDeriveSubject()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Session)
                .WithEmailTemplate(new EmailTemplateBuilder(this.Session).WithSubjectTemplate("subjectfromtemplate").Build())
                .WithSubject("subject")
                .Build();
            this.Session.Derive(false);

            emailCommunication.RemoveSubject();
            this.Session.Derive(false);

            Assert.Equal("subjectfromtemplate", emailCommunication.Subject);
        }

        [Fact]
        public void ChangedEmailTemplateDeriveSubject()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Session)
                .Build();
            this.Session.Derive(false);

            emailCommunication.EmailTemplate = new EmailTemplateBuilder(this.Session).WithSubjectTemplate("subjectfromtemplate").Build();
            this.Session.Derive(false);

            Assert.Equal("subjectfromtemplate", emailCommunication.Subject);
        }

        [Fact]
        public void ChangedSubjectDeriveWorkItemDescription()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Session)
                .Build();
            this.Session.Derive(false);

            emailCommunication.Subject = "subject";
            this.Session.Derive(false);

            Assert.Contains("subject", emailCommunication.WorkItemDescription);
        }

        [Fact]
        public void ChangedToEmailDeriveWorkItemDescription()
        {
            var emailCommunication = new EmailCommunicationBuilder(this.Session)
                .Build();
            this.Session.Derive(false);

            var to = new EmailAddressBuilder(this.Session).WithElectronicAddressString("email@something.com").Build();
            emailCommunication.ToEmail = to;
            this.Session.Derive(false);

            Assert.Contains("email@something.com", emailCommunication.WorkItemDescription);
        }
    }
}
