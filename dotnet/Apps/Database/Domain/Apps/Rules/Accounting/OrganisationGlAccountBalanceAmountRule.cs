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
                //m.OrganisationGlAccountBalance.RolePattern(v => v.DerivationTrigger, v => v.OrganisationGlAccount),
                m.AccountingTransactionDetail.RolePattern(v => v.Amount, v => v.OrganisationGlAccount),
                m.OrganisationGlAccount.AssociationPattern(v => v.AccountingTransactionDetailsWhereOrganisationGlAccount),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransactionDetail>())
            {
                var accountingPeriod = @this.AccountingTransactionWhereAccountingTransactionDetail.AccountingPeriod;

                var organisationGlAccount = @this.OrganisationGlAccount;
                var generalLedgerAccount = organisationGlAccount.GeneralLedgerAccount;

                var organisationGlAccountBalance = organisationGlAccount.
                    OrganisationGlAccountBalancesWhereOrganisationGlAccount
                    .FirstOrDefault(v => v.AccountingPeriod.Equals(accountingPeriod));

                if (organisationGlAccountBalance != null)
                {
                    if (generalLedgerAccount.RgsLevel == 4)
                    {
                        var details = organisationGlAccount.AccountingTransactionDetailsWhereOrganisationGlAccount.ToList();
                        var subDetails = new AccountingTransactionDetails(cycle.Transaction)
                            .Extent()
                            .Where(v => v.OrganisationGlAccount.GeneralLedgerAccount.Parent == generalLedgerAccount)
                            .ToList();

                        var allDetails = details.Concat(subDetails).ToList();

                        this.CalculateAmounts(organisationGlAccountBalance, allDetails);
                    }
                    else
                    {
                        var details = organisationGlAccount.AccountingTransactionDetailsWhereOrganisationGlAccount.ToList();

                        this.CalculateAmounts(organisationGlAccountBalance, details);

                        if (generalLedgerAccount.ExistParent)
                        {
                            organisationGlAccount.SubsidiaryOf.OrganisationGlAccountBalancesWhereOrganisationGlAccount.ToList()
                                .ForEach(v => v.DerivationTrigger = Guid.NewGuid());
                        }
                    }
                }
            }
        }

        private void CalculateAmounts(OrganisationGlAccountBalance organisationGlAccountBalance, IEnumerable<AccountingTransactionDetail> details)
        {
            organisationGlAccountBalance.CreditAmount = 0;
            organisationGlAccountBalance.DebitAmount = 0;

            foreach (var accountingTransactionDetail in details)
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
