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

    public class SalesOrderTransferDerivation : DomainDerivation
    {
        public SalesOrderTransferDerivation(M m) : base(m, new Guid("7E5895C6-712C-42F9-8B1C-964D8B8CBC1D")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.SalesOrderTransfer.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var salesOrderTransfer in matches.Cast<SalesOrderTransfer>())
            {
                if (salesOrderTransfer.ExistFrom && salesOrderTransfer.ExistInternalOrganisation && !salesOrderTransfer.ExistTo)
                {
                    var acl = new AccessControlLists(cycle.Session.Scope().User)[salesOrderTransfer.From];
                    if (!acl.CanExecute(M.SalesOrder.DoTransfer))
                    {
                        cycle.Validation.AddError($"{salesOrderTransfer} {salesOrderTransfer.Meta.To} No rights to transfer salesorder");
                    }
                    else
                    {
                        salesOrderTransfer.To = salesOrderTransfer.From.Clone(salesOrderTransfer.From.Meta.SalesOrderItems);
                        salesOrderTransfer.To.TakenBy = salesOrderTransfer.InternalOrganisation;

                        // TODO: Make sure 'from' customer is also a customer in 'to' internal organisation
                        if (!salesOrderTransfer.To.TakenBy.ActiveCustomers.Contains(salesOrderTransfer.To.BillToCustomer))
                        {
                            new CustomerRelationshipBuilder(salesOrderTransfer.Strategy.Session)
                                .WithInternalOrganisation(salesOrderTransfer.To.TakenBy)
                                .WithCustomer(salesOrderTransfer.To.BillToCustomer)
                                .Build();
                        }

                        //TODO: ShipToCustomer

                        salesOrderTransfer.From.SalesOrderState = new SalesOrderStates(salesOrderTransfer.Strategy.Session).Transferred;
                    }
                }
            }
        }
    }
}
