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

    public class SalesOrderTransferDerivation : DomainDerivation
    {
        public SalesOrderTransferDerivation(M m) : base(m, new Guid("7E5895C6-712C-42F9-8B1C-964D8B8CBC1D")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrderTransfer.InternalOrganisation),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrderTransfer>())
            {
                if (@this.ExistFrom && @this.ExistInternalOrganisation && !@this.ExistTo)
                {
                    var acl = new DatabaseAccessControlLists(cycle.Transaction.Context().User)[@this.From];
                    if (!acl.CanExecute(this.M.SalesOrder.DoTransfer))
                    {
                        cycle.Validation.AddError($"{@this} {@this.Meta.To} No rights to transfer salesorder");
                    }
                    else
                    {
                        @this.To = @this.From.Clone(@this.From.Meta.SalesOrderItems);
                        @this.To.TakenBy = @this.InternalOrganisation;

                        // TODO: Make sure 'from' customer is also a customer in 'to' internal organisation
                        if (!@this.To.TakenBy.ActiveCustomers.Contains(@this.To.BillToCustomer))
                        {
                            new CustomerRelationshipBuilder(@this.Strategy.Transaction)
                                .WithInternalOrganisation(@this.To.TakenBy)
                                .WithCustomer(@this.To.BillToCustomer)
                                .Build();
                        }

                        //TODO: ShipToCustomer

                        @this.From.SalesOrderState = new SalesOrderStates(@this.Strategy.Transaction).Transferred;
                    }
                }
            }
        }
    }
}
