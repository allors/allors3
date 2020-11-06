// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class BudgetExtensionsCreateDerivation : DomainDerivation
    {
        public BudgetExtensionsCreateDerivation(M m) : base(m, new Guid("3ed232de-2231-4b8f-9c7b-01603a350268")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.Budget.Interface)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Budget>())
            {
                if (!@this.ExistBudgetState)
                {
                    @this.BudgetState = new BudgetStates(@this.Strategy.Session).Opened;
                }
            }
        }
    }
}
