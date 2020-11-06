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

    public class OwnCreditCardCreationDerivation : DomainDerivation
    {
        public OwnCreditCardCreationDerivation(M m) : base(m, new Guid("99263e6d-c6f2-486d-8dfb-e22604d3934d")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.OwnCreditCard.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<OwnCreditCard>())
            {
                if (!@this.ExistIsActive)
                {
                    @this.IsActive = true;
                }
            }
        }
    }
}
