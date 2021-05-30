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
    using Derivations.Rules;
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
                var acl = new DatabaseAccessControlLists(cycle.Transaction.Services().User)[@this.From];
                if (!acl.CanExecute(this.M.SalesOrder.DoTransfer))
                {
                    cycle.Validation.AddError($"{@this} {@this.Meta.ToInternalOrganisation} No rights to transfer salesorder");
                }
                else
                {
                    var tree = @this.From.Meta.Nodes(
                                v => v.SalesOrderItems.Node(
                                        w => w.SalesOrderItem.Nodes(
                                            x => x.DiscountAdjustments.Node(),
                                            x => x.SurchargeAdjustments.Node(),
                                            x => x.SalesTerms.Node(),
                                            x => x.OrderedWithFeatures.Node(
                                                y => y.OrderItem.Nodes(
                                                    z => z.DiscountAdjustments.Node(),
                                                    z => z.SurchargeAdjustments.Node(),
                                                    z => z.SalesTerms.Node()
                                                )
                                            )
                                        )
                                    ),
                                v => v.OrderAdjustments.Node(),
                                v => v.SalesTerms.Node(),
                                v => v.LocalisedComments.Node(),
                                v => v.ElectronicDocuments.Node());

                    @this.ToSalesOrder = @this.From.Clone(tree);
                    @this.ToSalesOrder.TakenBy = @this.ToInternalOrganisation;
                    @this.ToSalesOrder.RemoveOrderNumber();

                    @this.From.SalesOrderState = new SalesOrderStates(@this.Strategy.Transaction).Transferred;
                }
            }
        }
    }
}
