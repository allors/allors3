// <copyright file="EmailMessagesTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Configuration;
    using Xunit;

    public class EmailMessagesTests : DomainTest, IClassFixture<Fixture>
    {
        public EmailMessagesTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SendTransmitsQueuedMessagesAndMarksThemSent()
        {
            var mailer = (RecordingMailer)this.Transaction.Database.Services.Get<IMailer>();

            var first = new EmailMessageBuilder(this.Transaction)
                .WithRecipientEmailAddress("jane@example.com").WithSubject("One").WithBody("Body one").Build();
            var second = new EmailMessageBuilder(this.Transaction)
                .WithRecipientEmailAddress("john@example.com").WithSubject("Two").WithBody("Body two").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            new EmailMessages(this.Transaction).Send("noreply@example.com");

            Assert.Contains(first, mailer.Sent);
            Assert.Contains(second, mailer.Sent);
            Assert.True(first.ExistDateSent);
            Assert.True(second.ExistDateSent);
        }

        [Fact]
        public void SendParksMessageWhenTransportFails()
        {
            var mailer = (RecordingMailer)this.Transaction.Database.Services.Get<IMailer>();
            mailer.ThrowOnSend = true;

            var message = new EmailMessageBuilder(this.Transaction)
                .WithRecipientEmailAddress("jane@example.com").WithSubject("One").WithBody("Body one").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            new EmailMessages(this.Transaction).Send("noreply@example.com");

            // The transport failed after DateSending was committed but before DateSent: the message
            // is parked (not resent on the next run, which filters out DateSending), by design.
            Assert.True(message.ExistDateSending);
            Assert.False(message.ExistDateSent);
        }
    }
}
