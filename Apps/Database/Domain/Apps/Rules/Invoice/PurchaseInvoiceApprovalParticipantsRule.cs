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
    using Database.Derivations;

    public class PurchaseInvoiceApprovalParticipantsRule : Rule
    {
        public PurchaseInvoiceApprovalParticipantsRule(MetaPopulation m) : base(m, new Guid("24dd13f4-270e-4ed7-b1d2-0520e74a992a")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoiceApproval.RolePattern(v => v.DateClosed),
                m.PurchaseInvoiceApproval.RolePattern(v => v.PurchaseInvoice),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseInvoiceApproval>())
            {
                var participants = @this.ExistDateClosed
                    ? (IEnumerable<Person>)Array.Empty<Person>()
                    : new UserGroups(@this.Transaction()).Administrators.Members.Select(v => (Person)v).ToArray();
                @this.AssignParticipants(participants);
            }
        }
    }
}
