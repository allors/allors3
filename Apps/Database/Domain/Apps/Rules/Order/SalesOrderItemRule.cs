// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesOrderItemRule : Rule
    {
        public SalesOrderItemRule(MetaPopulation m) : base(m, new Guid("FEF4E104-A0F0-4D83-A248-A1A606D93E41")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.RolePattern(v => v.Product),
                m.SalesOrderItem.RolePattern(v => v.QuantityOrdered),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                // TODO: Use versioning
                // laten staan
                if (@this.ExistPreviousProduct && !@this.PreviousProduct.Equals(@this.Product))
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.Product} {ErrorMessages.SalesOrderItemProductChangeNotAllowed}");
                }
                else
                {
                    @this.PreviousProduct = @this.Product;
                }

                @this.PreviousQuantity = @this.QuantityOrdered;
                @this.PreviousProduct = @this.Product;
            }
        }
    }
}
