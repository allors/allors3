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

    public class InventoryItemTransactionCreateDerivation : DomainDerivation
    {
        public InventoryItemTransactionCreateDerivation(M m) : base(m, new Guid("e6a947d1-c908-4a2c-9dc8-05b2c53d515e")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.InventoryItemTransaction.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                if (!@this.ExistTransactionDate)
                {
                    @this.TransactionDate = @this.Session().Now();
                }
            }
        }
    }
}
