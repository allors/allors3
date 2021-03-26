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
    using Resources;

    public class JournalDerivation : DomainDerivation
    {
        public JournalDerivation(M m) : base(m, new Guid("c52af46b-1cbd-47cd-a00f-76aa5c232db3")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Journal, m.Journal.ContraAccount),
                new RolePattern(m.Journal, m.Journal.JournalType),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Journal>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ContraAccount.ExistJournalEntryDetailsWhereGeneralLedgerAccount
                    && @this.CurrentVersion.ExistContraAccount
                    && @this.ContraAccount != @this.CurrentVersion.ContraAccount)
                {
                    validation.AddError($"{@this} {this.M.Journal.ContraAccount} {ErrorMessages.ContraAccountChanged}");
                }

                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ContraAccount.ExistJournalEntryDetailsWhereGeneralLedgerAccount
                    && @this.CurrentVersion.ExistJournalType
                    && @this.JournalType != @this.CurrentVersion.JournalType)
                {
                    validation.AddError($"{@this} {this.M.Journal.JournalType} {ErrorMessages.JournalTypeChanged}");
                }
            }
        }
    }
}
