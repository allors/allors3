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
    using Derivations;

    public class CashRule : Rule
    {
        public CashRule(M m) : base(m, new Guid("5b0365cc-0b8e-4ee1-89c5-c5955e3ce44c")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Cash, m.Cash.GeneralLedgerAccount),
                new RolePattern(m.Cash, m.Cash.Journal),
                new AssociationPattern(m.InternalOrganisation.DerivedActiveCollectionMethods) { OfType = m.Cash.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Cash>())
            {
                if (@this.InternalOrganisationWhereDerivedActiveCollectionMethod?.DoAccounting ?? false)
                {
                    validation.AssertAtLeastOne(@this, @this.Meta.GeneralLedgerAccount, @this.Meta.Journal);
                }

                validation.AssertExistsAtMostOne(@this, @this.Meta.GeneralLedgerAccount, @this.Meta.Journal);
            }
        }
    }
}
