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
    using Resources;

    public class JournalContraAccountRule : Rule
    {
        public JournalContraAccountRule(MetaPopulation m) : base(m, new Guid("727d7093-3c82-49cc-89c4-958dd52e7912")) =>
            this.Patterns = new Pattern[]
            {
                m.Journal.RolePattern(v => v.ContraAccount),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Journal>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ContraAccount.ExistAccountingTransactionDetailsWhereOrganisationGlAccount
                    && @this.CurrentVersion.ExistContraAccount
                    && @this.ContraAccount != @this.CurrentVersion.ContraAccount)
                {
                    validation.AddError($"{@this} {this.M.Journal.ContraAccount} {ErrorMessages.ContraAccountChanged}");
                }
            }
        }
    }
}
