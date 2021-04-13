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
    using Allors.Database.Data;

    public class SalesOrderTransferRule : Rule
    {
        public SalesOrderTransferRule(MetaPopulation m) : base(m, new Guid("7E5895C6-712C-42F9-8B1C-964D8B8CBC1D")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderTransfer.RolePattern(v => v.ToInternalOrganisation),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrderTransfer>().Where(v => v.ExistFrom && v.From.SalesOrderState.IsProvisional && v.ExistToInternalOrganisation && !v.ExistToSalesOrder))
            {
                var acl = new DatabaseAccessControlLists(cycle.Transaction.Context().User)[@this.From];
                if (!acl.CanExecute(this.M.SalesOrder.DoTransfer))
                {
                    cycle.Validation.AddError($"{@this} {@this.Meta.ToInternalOrganisation} No rights to transfer salesorder");
                }
                else
                {
                    @this.ToSalesOrder = @this.From.Clone(@this.From.Meta.SalesOrderItems.Node());
                    @this.ToSalesOrder.TakenBy = @this.ToInternalOrganisation;
                    @this.ToSalesOrder.RemoveOrderNumber();

                    @this.From.SalesOrderState = new SalesOrderStates(@this.Strategy.Transaction).Transferred;
                }
            }
        }
    }
}
