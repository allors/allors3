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
                new ChangedPattern(m.Journal.PreviousContraAccount),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Journal>())
            {
                AppsOnDeriveContraAccount(@this);
                AppsOnDerivePreviousJournalType(@this);
            }

            void AppsOnDeriveContraAccount(Journal journal)
            {
                if (journal.ExistPreviousContraAccount)
                {
                    if (journal.PreviousContraAccount.ExistJournalEntryDetailsWhereGeneralLedgerAccount)
                    {
                        validation.AssertAreEqual(journal, journal.Meta.ContraAccount, journal.Meta.PreviousContraAccount);
                    }
                    else
                    {
                        journal.PreviousContraAccount = journal.ContraAccount;
                    }
                }
                else
                {
                    if (journal.ExistJournalType && journal.JournalType.Equals(new JournalTypes(journal.Strategy.Session).Bank))
                    {
                        // initial derivation of ContraAccount, PreviousContraAccount does not exist yet.
                        if (journal.ExistContraAccount)
                        {
                            var savedContraAccount = journal.ContraAccount;
                            journal.RemoveContraAccount();
                            if (!savedContraAccount.IsNeutralAccount())
                            {
                                validation.AddError($"{journal}, {journal.Meta.ContraAccount}, {ErrorMessages.GeneralLedgerAccountNotNeutral}");
                            }

                            if (!savedContraAccount.GeneralLedgerAccount.BalanceSheetAccount)
                            {
                                validation.AddError($"{journal}, {journal.Meta.ContraAccount}, {ErrorMessages.GeneralLedgerAccountNotBalanceAccount}");
                            }

                            journal.ContraAccount = savedContraAccount;
                        }
                    }
                    // TODO: How to get HasErrors
                    //if (!validation.HasErrors)
                    //{
                    //    journal.PreviousContraAccount = journal.ContraAccount;
                    //}
                }
            }

            void AppsOnDerivePreviousJournalType(Journal journal)
            {
                if (journal.ExistPreviousJournalType)
                {
                    if (journal.ExistPreviousContraAccount && journal.PreviousContraAccount.ExistJournalEntryDetailsWhereGeneralLedgerAccount)
                    {
                        validation.AssertAreEqual(journal, journal.Meta.JournalType, journal.Meta.PreviousJournalType);
                    }
                    else
                    {
                        journal.PreviousJournalType = journal.JournalType;
                    }
                }
                else
                {
                    journal.PreviousJournalType = journal.JournalType;
                }
            }
        }
    }
}
