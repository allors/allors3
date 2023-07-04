// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class AccountingTransactionShipmentNumberRule : Rule
    {
        public AccountingTransactionShipmentNumberRule(MetaPopulation m) : base(m, new Guid("196ec5f6-bffc-4f6e-83e9-161b58445dbd")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.Shipment),
                m.Shipment.RolePattern(v => v.ShipmentNumber, v => v.AccountingTransactionsWhereShipment.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionShipmentNumber(validation);
            }
        }
    }

    public static class AccountingTransactionShipmentNumberRuleExtensions
    {
        public static void DeriveAccountingTransactionShipmentNumber(this AccountingTransaction @this, IValidation validation) => @this.ShipmentNumber = @this.Shipment.ShipmentNumber;
    }
}
