// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class EmailCommunicationDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("21A5FF76-FB80-4CA3-B3C4-A79066BADA8E");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.EmailCommunication.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (EmailCommunication emailCommunication in matches.Cast<EmailCommunication>())
            {
                if (!emailCommunication.ExistSubject && emailCommunication.ExistEmailTemplate && emailCommunication.EmailTemplate.ExistSubjectTemplate)
                {
                    emailCommunication.Subject = emailCommunication.EmailTemplate.SubjectTemplate;
                }

                emailCommunication.WorkItemDescription = $"Email to {emailCommunication.ToEmail} about {emailCommunication.Subject}";
            }
        }
    }
}
