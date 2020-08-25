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
        public class EmailTemplateCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdEmailTemplates = changeSet.Created.Select(session.Instantiate).OfType<EmailTemplate>();

                foreach (EmailTemplate emailTemplate in createdEmailTemplates)
                {
                    if (!emailTemplate.ExistDescription)
                    {
                        emailTemplate.Description = "Default";
                    }
                }
            }
        }

        public static void EmailTemplateRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("44128CB5-4171-4606-A091-A790AFE721AB")] = new EmailTemplateCreationDerivation();
        }
    }
}
