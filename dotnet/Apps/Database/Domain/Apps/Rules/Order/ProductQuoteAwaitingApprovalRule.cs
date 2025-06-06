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

    public class ProductQuoteAwaitingApprovalRule : Rule
    {
        public ProductQuoteAwaitingApprovalRule(MetaPopulation m) : base(m, new Guid("bd798939-8109-4b81-af62-f37be1042091")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductQuote.RolePattern(v => v.QuoteState),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductQuote>().Where(v => v.ExistQuoteState && v.QuoteState.IsAwaitingApproval))
            {
                var openTasks = @this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

                if (!openTasks.OfType<ProductQuoteApproval>().Any())
                {
                    new ProductQuoteApprovalBuilder(@this.Transaction()).WithProductQuote(@this).Build();
                }
            }
        }
    }
}
