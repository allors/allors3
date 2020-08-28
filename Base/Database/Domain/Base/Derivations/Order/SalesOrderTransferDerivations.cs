// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class SalesOrderTransferCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSalesOrderTransfers = changeSet.Created.Select(session.Instantiate).OfType<SalesOrderTransfer>();

                foreach (var salesOrderTransfer in createdSalesOrderTransfers)
                {
                    if (salesOrderTransfer.ExistFrom && salesOrderTransfer.ExistInternalOrganisation && !salesOrderTransfer.ExistTo)
                    {
                        var acl = new AccessControlLists(session.GetUser())[salesOrderTransfer.From];
                        if (!acl.CanExecute(M.SalesOrder.DoTransfer))
                        {
                            validation.AddError($"{salesOrderTransfer} {salesOrderTransfer.Meta.To} No rights to transfer salesorder");
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

        public static void SalesOrderTransferRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("68d5dbb7-0eca-43ff-8544-2e87531b1e76")] = new SalesOrderTransferCreationDerivation();
        }
    }
}
