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
                            // A level-4 balance aggregates the details booked on its level-5 children
                            // (GeneralLedgerAccountsWhereParent), together with any booked on the level-4 account itself.
                            var subDetails = internalOrganisation.AccountingTransactionsWhereInternalOrganisation
                                .Where(t => t.AccountingPeriod == accountingPeriod)
                                .SelectMany(t => t.AccountingTransactionDetails.Where(d => generalLedgerAccount.GeneralLedgerAccountsWhereParent.Contains(d.GeneralLedgerAccount)))
                                .ToList();

                            var allDetails = details.Concat(subDetails).ToList();

                            this.CalculateAmounts(organisationGlAccountBalance, allDetails);
                        }
                        else
                        {
                            this.CalculateAmounts(organisationGlAccountBalance, details);

                            // Roll the change up into the parent (level-4) balance, which aggregates this account's
                            // siblings. The parent is reached through OrganisationGlAccount.SubsidiaryOf.
                            if (organisationGlAccount.ExistSubsidiaryOf)
                            {
                                var parentOrganisationGlAccount = organisationGlAccount.SubsidiaryOf;
                                var parentGeneralLedgerAccount = parentOrganisationGlAccount.GeneralLedgerAccount;

                                var parentBalance = parentOrganisationGlAccount
                                    .OrganisationGlAccountBalancesWhereOrganisationGlAccount
                                    .FirstOrDefault(v => v.AccountingPeriod.Equals(accountingPeriod));

                                if (parentBalance != null)
                                {
                                    var parentDetails = internalOrganisation.AccountingTransactionsWhereInternalOrganisation
                                        .Where(t => t.AccountingPeriod == accountingPeriod)
                                        .SelectMany(t => t.AccountingTransactionDetails.Where(d =>
                                            d.GeneralLedgerAccount.Equals(parentGeneralLedgerAccount)
                                            || parentGeneralLedgerAccount.GeneralLedgerAccountsWhereParent.Contains(d.GeneralLedgerAccount)))
                                        .ToList();

                                    this.CalculateAmounts(parentBalance, parentDetails);
                                }
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
