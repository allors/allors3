// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class RequestItemCreationDerivation : DomainDerivation
    {
        public RequestItemCreationDerivation(M m) : base(m, new Guid("6f84c187-636a-4f15-9370-ac82ccdff499")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.RequestItem.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<RequestItem>())
            {
                if (!@this.ExistRequestItemState)
                {
                    @this.RequestItemState = new RequestItemStates(@this.Strategy.Session).Submitted;
                }
            }
        }
    }
}
