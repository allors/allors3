// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class EmailCommunicationCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdEmailCommunication = changeSet.Created.Select(session.Instantiate).OfType<EmailCommunication>();

                foreach(EmailCommunication emailCommunication in createdEmailCommunication)
                {
                    if (!emailCommunication.ExistSubject && emailCommunication.ExistEmailTemplate && emailCommunication.EmailTemplate.ExistSubjectTemplate)
                    {
                        emailCommunication.Subject = emailCommunication.EmailTemplate.SubjectTemplate;
                    }

                    emailCommunication.WorkItemDescription = $"Email to {emailCommunication.ToEmail} about {emailCommunication.Subject}";
                }
            }
        }

        public static void EmailCommunicationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("6aca95a5-4b40-403f-b9e2-0cc7d39cc621")] = new EmailCommunicationCreationDerivation();
        }
    }
}
