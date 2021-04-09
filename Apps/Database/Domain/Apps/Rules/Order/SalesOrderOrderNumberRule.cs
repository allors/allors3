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

    public class SalesOrderOrderNumberRule : Rule
    {
        public SalesOrderOrderNumberRule(MetaPopulation m) : base(m, new Guid("85f5c0dd-0df4-4f55-b3e7-fdce5c172f28")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.Store),
                m.SalesOrder.RolePattern(v => v.OrderDate),
                m.SalesOrder.RolePattern(v => v.TakenBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                if (!@this.ExistOrderNumber && @this.ExistStore)
                {
                    var year = @this.OrderDate.Year;
                    @this.OrderNumber = @this.Store.NextSalesOrderNumber(year);

                    var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.TakenBy.InvoiceSequence.IsEnforcedSequence ? @this.Store.SalesOrderNumberPrefix : fiscalYearStoreSequenceNumbers.SalesOrderNumberPrefix;
                    @this.SortableOrderNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.OrderNumber, year.ToString());
                }
            }
        }
    }
}
