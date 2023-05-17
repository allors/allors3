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
    using Resources;

    public class JournalTypeRule : Rule
    {
        public JournalTypeRule(MetaPopulation m) : base(m, new Guid("c52af46b-1cbd-47cd-a00f-76aa5c232db3")) =>
            this.Patterns = new Pattern[]
            {
                m.Journal.RolePattern(v => v.JournalType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Journal>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ContraAccount.ExistAccountingTransactionDetailsWhereGeneralLedgerAccount
                    && @this.CurrentVersion.ExistJournalType
                    && @this.JournalType != @this.CurrentVersion.JournalType)
                {
                    validation.AddError(@this, this.M.Journal.JournalType, ErrorMessages.JournalTypeChanged);
                }
            }
        }
    }
}
