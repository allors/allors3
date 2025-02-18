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

    public class GeneralLedgerAccountDisplayNameRule : Rule
    {
        public GeneralLedgerAccountDisplayNameRule(MetaPopulation m) : base(m, new Guid("374f2114-8c57-4032-b859-b2a8de855050")) =>
            this.Patterns = new Pattern[]
            {
                m.GeneralLedgerAccount.RolePattern(v => v.ReferenceNumber),
                m.GeneralLedgerAccount.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                @this.DeriveGeneralLedgerAccountDisplayName(validation);
            }
        }
    }
    public static class GeneralLedgerAccountDisplayNameRuleExtensions
    {
        public static void DeriveGeneralLedgerAccountDisplayName(this GeneralLedgerAccount @this, IValidation validation)
        {
            @this.DisplayName = $"{@this.ReferenceNumber}: {@this.Name}";
        }
    }
}
