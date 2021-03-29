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

    public class PurchaseOrderApprovalLevel2ParticipantsRule : Rule
    {
        public PurchaseOrderApprovalLevel2ParticipantsRule(M m) : base(m, new Guid("f2853dc1-fcf3-4a0b-82a3-0a514f7aa13e")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrderApprovalLevel2, m.PurchaseOrderApprovalLevel2.DateClosed),
                new RolePattern(m.PurchaseOrderApprovalLevel2, m.PurchaseOrderApprovalLevel2.PurchaseOrder)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderApprovalLevel2>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : new UserGroups(@this.Transaction()).Administrators.Members.Select(v => (Person)v).ToArray();
                @this.AssignParticipants(participants);
            }
        }
    }
}
