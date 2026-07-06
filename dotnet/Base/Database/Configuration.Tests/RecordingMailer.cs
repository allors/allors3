// <copyright file="RecordingMailer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using Domain;

    // Test IMailer that records the messages it is asked to send instead of connecting to SMTP.
    // Set ThrowOnSend to simulate a transport failure (exercises the queue's poison-parking path).
    public class RecordingMailer : IMailer
    {
        public List<EmailMessage> Sent { get; } = new List<EmailMessage>();

        public bool ThrowOnSend { get; set; }

        public void Send(EmailMessage emailMesssage, string defaultSender = null)
        {
            if (this.ThrowOnSend)
            {
                throw new Exception("RecordingMailer is configured to throw.");
            }

            this.Sent.Add(emailMesssage);
        }
    }
}
