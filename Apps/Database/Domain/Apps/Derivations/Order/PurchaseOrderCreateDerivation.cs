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

    public class PurchaseOrderCreateDerivation : DomainDerivation
    {
        public PurchaseOrderCreateDerivation(M m) : base(m, new Guid("061b52ca-6644-4cb6-af28-28a078ffb66f")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.PurchaseOrder.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.OrderDate = @this.Session().Now();

                @this.PurchaseOrderState ??= new PurchaseOrderStates(@this.Strategy.Session).Created;
                @this.PurchaseOrderShipmentState ??= new PurchaseOrderShipmentStates(@this.Strategy.Session).NotReceived;
                @this.PurchaseOrderPaymentState ??= new PurchaseOrderPaymentStates(@this.Strategy.Session).NotPaid;

                if (!@this.ExistEntryDate)
                {
                    @this.EntryDate = @this.Session().Now();
                }

                if (!@this.ExistOrderedBy)
                {
                    var internalOrganisations = new Organisations(@this.Strategy.Session).InternalOrganisations();
                    if (internalOrganisations.Count() == 1)
                    {
                        @this.OrderedBy = internalOrganisations.First();
                    }
                }

                if (!@this.ExistCurrency)
                {
                    @this.Currency = @this.OrderedBy?.PreferredCurrency;
                }

                if (!@this.ExistStoredInFacility && @this.OrderedBy?.StoresWhereInternalOrganisation.Count == 1)
                {
                    @this.StoredInFacility = @this.OrderedBy.StoresWhereInternalOrganisation.Single().DefaultFacility;
                }
            }
        }
    }
}
