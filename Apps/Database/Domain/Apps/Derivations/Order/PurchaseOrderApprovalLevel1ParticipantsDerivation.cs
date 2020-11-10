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

    public class PurchaseOrderApprovalLevel1ParticipantsDerivation : DomainDerivation
    {
        public PurchaseOrderApprovalLevel1ParticipantsDerivation(M m) : base(m, new Guid("bbf7c4c8-9af9-49a3-84bb-557c3be542a7")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.PurchaseOrderApprovalLevel1.DateClosed)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderApprovalLevel1>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : new UserGroups(@this.Session()).Administrators.Members.Select(v => (Person)v).ToArray();
                @this.AssignParticipants(participants);
            }
        }
    }
}
