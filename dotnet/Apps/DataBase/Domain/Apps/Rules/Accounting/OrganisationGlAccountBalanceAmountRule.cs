namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class OrganisationGlAccountBalanceAmountRule : Rule
    {
        public OrganisationGlAccountBalanceAmountRule(MetaPopulation m) : base(m, new Guid("aebf3ae1-4654-44dd-b4d3-a025afb203de")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransactionDetail.RolePattern(v => v.Amount),
                m.OrganisationGlAccount.AssociationPattern(v => v.AccountingTransactionDetailsWhereOrganisationGlAccount, v => v.AccountingTransactionDetailsWhereOrganisationGlAccount),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransactionDetail>())
            {
                var accountingPeriod = @this.AccountingTransactionWhereAccountingTransactionDetail.AccountingPeriod;

                var organisationGlAccount = @this.OrganisationGlAccount;

                var organisationGlAccountBalance = organisationGlAccount.
                    OrganisationGlAccountBalancesWhereOrganisationGlAccount.
                    FirstOrDefault(v => v.AccountingPeriod.Equals(accountingPeriod));

                if (organisationGlAccountBalance == null)
                {
                    continue;
                }

                organisationGlAccountBalance.CreditAmount = 0;
                organisationGlAccountBalance.DebitAmount = 0;

                foreach (var accountingTransactionDetail in organisationGlAccount.AccountingTransactionDetailsWhereOrganisationGlAccount)
                {
                    if (accountingTransactionDetail.BalanceSide.IsCredit)
                    {
                        organisationGlAccountBalance.CreditAmount += accountingTransactionDetail.Amount;
                    }
                    else
                    {
                        organisationGlAccountBalance.DebitAmount += accountingTransactionDetail.Amount;
                    }
                }

                organisationGlAccountBalance.Amount = organisationGlAccountBalance.DebitAmount - organisationGlAccountBalance.CreditAmount;
            }
        }
    }
}
