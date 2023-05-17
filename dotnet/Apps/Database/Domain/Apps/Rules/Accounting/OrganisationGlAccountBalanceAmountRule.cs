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
                m.AccountingTransactionDetail.RolePattern(v => v.GeneralLedgerAccount),
                m.GeneralLedgerAccount.AssociationPattern(v => v.AccountingTransactionDetailsWhereGeneralLedgerAccount, v => v.AccountingTransactionDetailsWhereGeneralLedgerAccount)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransactionDetail>())
            {
                if (@this.AccountingTransactionWhereAccountingTransactionDetail.ExistAccountingPeriod)
                {
                    var internalOrganisation = @this.AccountingTransactionWhereAccountingTransactionDetail.InternalOrganisation;
                    var accountingPeriod = @this.AccountingTransactionWhereAccountingTransactionDetail.AccountingPeriod;

                    var generalLedgerAccount = @this.GeneralLedgerAccount;
                    var organisationGlAccount = @this.GeneralLedgerAccount.OrganisationGlAccountsWhereGeneralLedgerAccount.First(v => v.InternalOrganisation.Equals(internalOrganisation));
                    var details = internalOrganisation.AccountingTransactionsWhereInternalOrganisation.Where(v => v.AccountingPeriod == accountingPeriod).SelectMany(v => v.AccountingTransactionDetails.Where(v => v.GeneralLedgerAccount.Equals(generalLedgerAccount))).ToList();

                    var organisationGlAccountBalance = organisationGlAccount.
                        OrganisationGlAccountBalancesWhereOrganisationGlAccount
                        .FirstOrDefault(v => v.AccountingPeriod.Equals(accountingPeriod));

                    if (organisationGlAccountBalance != null)
                    {
                        if (generalLedgerAccount.RgsLevel == 4)
                        {
                            var subDetails = internalOrganisation.AccountingTransactionsWhereInternalOrganisation.Where(v => v.AccountingPeriod == accountingPeriod).SelectMany(v => v.AccountingTransactionDetails.Where(v => v.GeneralLedgerAccount.Equals(generalLedgerAccount.Parent))).ToList();

                            var allDetails = details.Concat(subDetails).ToList();

                            this.CalculateAmounts(organisationGlAccountBalance, allDetails);
                        }
                        else
                        {
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
