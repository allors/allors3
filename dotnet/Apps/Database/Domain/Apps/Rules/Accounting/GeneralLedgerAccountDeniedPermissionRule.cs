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

    public class GeneralLedgerAccountDeniedPermissionRule : Rule
    {
        public GeneralLedgerAccountDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("5a51005b-c1ed-432f-a6cd-cd73375bd6fb")) =>
            this.Patterns = new Pattern[]
        {
            m.AccountingTransactionDetail.RolePattern(v => v.GeneralLedgerAccount, v => v.GeneralLedgerAccount),
            m.InternalOrganisation.AssociationPattern(v => v.AccountingTransactionsWhereInternalOrganisation, v => v.OrganisationGlAccountsWhereInternalOrganisation.OrganisationGlAccount.GeneralLedgerAccount.GeneralLedgerAccount),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                @this.DeriveGeneralLedgerAccountDeniedPermission(validation);
            }
        }
    }

    public static class GeneralLedgerAccountDeniedPermissionRuleExtensions
    {
        public static void DeriveGeneralLedgerAccountDeniedPermission(this GeneralLedgerAccount @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).GeneralLedgerAccountDeleteRevocation;

            if (@this.IsDeletable)
            {
                @this.RemoveRevocation(deleteRevocation);
            }
            else
            {
                @this.AddRevocation(deleteRevocation);
            }
        }
    }
}
