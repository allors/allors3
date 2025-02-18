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

    public class OrganisationGlAccountDeniedPermissionRule : Rule
    {
        public OrganisationGlAccountDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("fb99f1f6-2783-4f94-89b9-bfc4cae4bec9")) =>
            this.Patterns = new Pattern[]
        {
            m.GeneralLedgerAccount.AssociationPattern(v => v.AccountingTransactionDetailsWhereGeneralLedgerAccount, v => v.OrganisationGlAccountsWhereGeneralLedgerAccount.ObjectType),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OrganisationGlAccount>())
            {
                @this.DeriveOrganisationGlAccountDeniedPermission(validation);
            }
        }
    }

    public static class OrganisationGlAccountDeniedPermissionRuleExtensions
    {
        public static void DeriveOrganisationGlAccountDeniedPermission(this OrganisationGlAccount @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).OrganisationGlAccountDeleteRevocation;

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
