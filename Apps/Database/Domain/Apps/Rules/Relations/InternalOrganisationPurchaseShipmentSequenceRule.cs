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

    public class InternalOrganisationPurchaseShipmentSequenceRule : Rule
    {
        public InternalOrganisationPurchaseShipmentSequenceRule(MetaPopulation m) : base(m, new Guid("ce8ae4b1-182d-4f35-813e-7861c29e97db")) =>
            this.Patterns = new Pattern[]
            {
                m.InternalOrganisation.RolePattern(v => v.PurchaseShipmentSequence),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                //Altijd
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (@this.PurchaseShipmentSequence != new PurchaseShipmentSequences(@this.Strategy.Transaction).RestartOnFiscalYear && !@this.ExistPurchaseShipmentNumberCounter)
                    {
                        @this.PurchaseShipmentNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }
                }
            }
        }
    }
}
