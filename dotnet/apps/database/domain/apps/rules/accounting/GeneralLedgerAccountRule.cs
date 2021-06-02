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
    using Derivations.Rules;
    using Resources;

    public class GeneralLedgerAccountRule : Rule
    {
        public GeneralLedgerAccountRule(MetaPopulation m) : base(m, new Guid("e916d6c3-b31b-41e2-b7ef-3265977e0fea")) =>
            this.Patterns = new Pattern[]
            {
                m.GeneralLedgerAccount.RolePattern(v => v.ReferenceNumber),
                m.GeneralLedgerAccount.AssociationPattern(v => v.ChartOfAccountsWhereGeneralLedgerAccount),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<GeneralLedgerAccount>())
            {
                if (@this.ExistChartOfAccountsWhereGeneralLedgerAccount)
                {
                    var generalLedgerAccounts = @this.ChartOfAccountsWhereGeneralLedgerAccount.GeneralLedgerAccounts.Where(v => v.ReferenceNumber == @this.ReferenceNumber);

                    if (generalLedgerAccounts.Count() > 1)
                    {
                        validation.AddError(@this, @this.Meta.ReferenceNumber, ErrorMessages.AccountNumberUniqueWithinChartOfAccounts);
                    }
                }
            }
        }
    }
}
