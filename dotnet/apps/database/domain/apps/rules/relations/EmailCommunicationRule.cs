// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class EmailCommunicationRule : Rule
    {
        public EmailCommunicationRule(MetaPopulation m) : base(m, new Guid("21A5FF76-FB80-4CA3-B3C4-A79066BADA8E")) =>
            this.Patterns = new Pattern[]
            {
                m.EmailCommunication.RolePattern(v => v.Subject),
                m.EmailCommunication.RolePattern(v => v.EmailTemplate),
                m.EmailCommunication.RolePattern(v => v.ToEmail),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<EmailCommunication>())
            {
                if (!@this.ExistSubject && @this.ExistEmailTemplate && @this.EmailTemplate.ExistSubjectTemplate)
                {
                    @this.Subject = @this.EmailTemplate.SubjectTemplate;
                }

                @this.WorkItemDescription = $"Email to {@this.ToEmail} about {@this.Subject}";
            }
        }
    }
}
