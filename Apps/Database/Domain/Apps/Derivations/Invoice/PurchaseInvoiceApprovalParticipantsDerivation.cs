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

    public class PurchaseInvoiceApprovalParticipantsDerivation : DomainDerivation
    {
        public PurchaseInvoiceApprovalParticipantsDerivation(M m) : base(m, new Guid("24dd13f4-270e-4ed7-b1d2-0520e74a992a")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.PurchaseInvoiceApproval.DateClosed)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseInvoiceApproval>())
            {
                var participants = @this.ExistDateClosed
                    ? (IEnumerable<Person>)Array.Empty<Person>()
                    : new UserGroups(@this.Session()).Administrators.Members.Select(v => (Person)v).ToArray();
                @this.AssignParticipants(participants);
            }
        }
    }
}
