// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectsBase.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Allors.Integration.Transform
{
    public partial class GeneralLedgerAccountTransformer
    {
        public GeneralLedgerAccountTransformer(Source.Source source, Population population, ILoggerFactory loggerFactory)
        {
            this.Source = source;
            this.Population = population;
            this.LoggerFactory = loggerFactory;
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccountTransformer>();
        }

        public Source.Source Source { get; }

        public Population Population { get; }

        public ILoggerFactory LoggerFactory { get; }

        public ILogger<GeneralLedgerAccountTransformer> Logger { get; set; }

        public void Execute(out Staging.GeneralLedgerAccount[] generalLedgerAccounts)
        {
            var generalLedgerAccountsList = new List<Staging.GeneralLedgerAccount>();
            Staging.GeneralLedgerAccount latestNiveau2Account = null;
            Staging.GeneralLedgerAccount previousNiveau2Account = null;

            foreach (var generalLedgerAccount in this.Source.GeneralLedgerAccounts)
            {
                var newGeneralLedgerAccount = new Staging.GeneralLedgerAccount()
                {
                    ReferenceCode = generalLedgerAccount.ReferenceCode,
                    SortCode = generalLedgerAccount.SortCode,
                    ReferenceNumber = generalLedgerAccount.ReferenceNumber,
                    Name = generalLedgerAccount.Name,
                    Description = generalLedgerAccount.Description,
                    GeneralLedgerAccountType = latestNiveau2Account.ReferenceCode,
                    CounterPartAccount = generalLedgerAccount.CounterPartAccount,
                    Parent = previousNiveau2Account.ReferenceCode,
                    BalanceSide = generalLedgerAccount.BalanceSide,
                    BalanceType = (generalLedgerAccount.ReferenceCode[0] == 'B') ? "Balance" : "ProfitLoss",
                    RgsLevel = generalLedgerAccount.Level,
                    IsRgsUseWithZzp = generalLedgerAccount.IsRgsUseWithZzp,
                    IsRgsBase = generalLedgerAccount.IsRgsBase,
                    IsRgsExtended = generalLedgerAccount.IsRgsExtended,
                    IsRgsUseWithEZ = generalLedgerAccount.IsRgsUseWithEZ,
                    IsRgsUseWithWoco = generalLedgerAccount.IsRgsUseWithWoco,
                    ExcludeRgsBB = generalLedgerAccount.ExcludeRgsBB,
                    ExcludeRgsAgro = generalLedgerAccount.ExcludeRgsAgro,
                    ExcludeRgsWKR = generalLedgerAccount.ExcludeRgsWKR,
                    ExcludeRgsEZVOF = generalLedgerAccount.ExcludeRgsEZVOF,
                    ExcludeRgsBV = generalLedgerAccount.ExcludeRgsBV,
                    ExcludeRgsWoco = generalLedgerAccount.ExcludeRgsWoco,
                    ExcludeRgsBank = generalLedgerAccount.ExcludeRgsBank,
                    ExcludeRgsOZW = generalLedgerAccount.ExcludeRgsOZW,
                    ExcludeRgsAfrekSyst = generalLedgerAccount.ExcludeRgsAfrekSyst,
                    ExcludeRgsNivo5 = generalLedgerAccount.ExcludeRgsNivo5,
                    ExcludeRgsUitbr5 = generalLedgerAccount.ExcludeRgsUitbr5,
                };

                if (generalLedgerAccount.Level == 2)
                {
                    newGeneralLedgerAccount.GeneralLedgerAccountType = newGeneralLedgerAccount.ReferenceCode;
                    latestNiveau2Account = newGeneralLedgerAccount;
                }

                generalLedgerAccountsList.Add(newGeneralLedgerAccount);
                previousNiveau2Account = newGeneralLedgerAccount;
            }

            generalLedgerAccounts = generalLedgerAccountsList.ToArray();
        }
    }
}
