// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class PurchaseOrderApprovalLevel2ParticipantsRule : Rule
    {
        public PurchaseOrderApprovalLevel2ParticipantsRule(MetaPopulation m) : base(m, new Guid("f2853dc1-fcf3-4a0b-82a3-0a514f7aa13e")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderApprovalLevel2.RolePattern(v => v.DateClosed),
                m.PurchaseOrderApprovalLevel2.RolePattern(v => v.PurchaseOrder)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
