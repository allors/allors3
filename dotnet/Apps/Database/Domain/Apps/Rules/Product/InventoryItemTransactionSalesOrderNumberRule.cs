// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class InventoryItemTransactionSalesOrderNumberRule : Rule
    {
        public InventoryItemTransactionSalesOrderNumberRule(MetaPopulation m) : base(m, new Guid("c12d37a2-635f-4ce2-af73-3ad871f3b27d")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItemInventoryAssignment.RolePattern(v => v.InventoryItemTransactions, v => v.InventoryItemTransactions),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                @this.DeriveInventoryItemTransactionSalesOrderNumber(validation);
            }
        }
    }

    public static class InventoryItemTransactionSalesOrderNumberRuleExtensions
    {
        public static void DeriveInventoryItemTransactionSalesOrderNumber(this InventoryItemTransaction @this, IValidation validation)
        {
            // Assign only once
            if (!@this.ExistSalesOrderNumber)
            {
                @this.SalesOrderNumber = @this.SalesOrderItemInventoryAssignmentWhereInventoryItemTransaction?.SalesOrderItemWhereSalesOrderItemInventoryAssignment.SalesOrderWhereSalesOrderItem.OrderNumber;
            }
        }
    }
}
