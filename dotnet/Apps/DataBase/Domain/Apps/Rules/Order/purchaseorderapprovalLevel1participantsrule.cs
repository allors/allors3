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

    public class PurchaseOrderApprovalLevel1ParticipantsRule : Rule
    {
        public PurchaseOrderApprovalLevel1ParticipantsRule(MetaPopulation m) : base(m, new Guid("bbf7c4c8-9af9-49a3-84bb-557c3be542a7")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderApprovalLevel1.RolePattern(v => v.DateClosed),
                m.PurchaseOrderApprovalLevel1.RolePattern(v => v.PurchaseOrder),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderApprovalLevel1>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : new UserGroups(@this.Transaction()).Administrators.Members.Select(v => (Person)v).ToArray();
                @this.AssignParticipants(participants);
            }
        }
    }
}
