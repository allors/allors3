// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class SalesOrderCreateDerivation : DomainDerivation
    {
        public SalesOrderCreateDerivation(M m) : base(m, new Guid("cb3d6431-d6ab-4f24-a4b3-da4d19e2efff")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.SalesOrder.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                if (!@this.ExistSalesOrderState)
                {
                    @this.SalesOrderState = new SalesOrderStates(@this.Session()).Provisional;
                }

                if (!@this.ExistSalesOrderShipmentState)
                {
                    @this.SalesOrderShipmentState = new SalesOrderShipmentStates(@this.Session()).NotShipped;
                }

                if (!@this.ExistSalesOrderInvoiceState)
                {
                    @this.SalesOrderInvoiceState = new SalesOrderInvoiceStates(@this.Session()).NotInvoiced;
                }

                if (!@this.ExistSalesOrderPaymentState)
                {
                    @this.SalesOrderPaymentState = new SalesOrderPaymentStates(@this.Session()).NotPaid;
                }

                if (!@this.ExistOrderDate)
                {
                    @this.OrderDate = @this.Session().Now();
                }

                if (!@this.ExistEntryDate)
                {
                    @this.EntryDate = @this.Session().Now();
                }

                if (!@this.ExistTakenBy)
                {
                    var internalOrganisations = new Organisations(@this.Session()).InternalOrganisations();
                    if (internalOrganisations.Count() == 1)
                    {
                        @this.TakenBy = internalOrganisations.First();
                    }
                }

                if (!@this.ExistStore && @this.ExistTakenBy)
                {
                    var stores = new Stores(@this.Session()).Extent();
                    stores.Filter.AddEquals(this.M.Store.InternalOrganisation, @this.TakenBy);

                    if (stores.Any())
                    {
                        @this.Store = stores.First;
                    }
                }

                if (!@this.ExistOriginFacility)
                {
                    @this.OriginFacility = @this.ExistStore ? @this.Store.DefaultFacility : @this.Session().GetSingleton().Settings.DefaultFacility;
                }
            }
        }
    }
}
