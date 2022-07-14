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

        public void Execute(out Staging.GeneralLedgerAccount[] generalLedgerAccounts, out Staging.GeneralLedgerAccountClassification[] generalLedgerAccountClassifications, out Staging.GeneralLedgerAccountType[] generalLedgerAccountTypes)
        {
            var generalLedgerAccountsList = new List<Staging.GeneralLedgerAccount>();
            var generalLedgerAccountClassificationsList = new List<Staging.GeneralLedgerAccountClassification>();
            var generalLedgerAccountTypesList = new List<Staging.GeneralLedgerAccountType>();

            Staging.GeneralLedgerAccountClassification latestNiveau2AccountClassification = null;
            Staging.GeneralLedgerAccountType latestNiveau2AccountType = null;

            Staging.GeneralLedgerAccountClassification latestNiveau3AccountClassification = null;

            foreach (var generalLedgerAccount in this.Source.GeneralLedgerAccounts)
            {
                if (generalLedgerAccount.Level == 2)
                {
                    var classification = new Staging.GeneralLedgerAccountClassification()
                    {
                        ReferenceCode = generalLedgerAccount.ReferenceCode,
                        ReferenceNumber = generalLedgerAccount.ReferenceNumber, // TODO:
                        Name = generalLedgerAccount.Name,
                        RgsLevel = generalLedgerAccount.Level,
                        SortCode = generalLedgerAccount.SortCode,
                        Parent = null,
                    };

                    var type = new Staging.GeneralLedgerAccountType()
                    {
                        Description = generalLedgerAccount.Name,
                    };

                    latestNiveau2AccountClassification = classification;
                    latestNiveau2AccountType = type;

                    generalLedgerAccountClassificationsList.Add(classification);
                    generalLedgerAccountTypesList.Add(type);
                }
                else if (generalLedgerAccount.Level == 3)
                {
                    var classification = new Staging.GeneralLedgerAccountClassification()
                    {
                        ReferenceCode = generalLedgerAccount.ReferenceCode,
                        ReferenceNumber = generalLedgerAccount.ReferenceNumber, // TODO:
                        Name = generalLedgerAccount.Name,
                        RgsLevel = generalLedgerAccount.Level,
                        SortCode = generalLedgerAccount.SortCode,
                        Parent = latestNiveau2AccountClassification.ReferenceCode,
                    };

                    latestNiveau3AccountClassification = classification;

                    generalLedgerAccountClassificationsList.Add(classification);
                }
                else if (generalLedgerAccount.Level == 4)
                {
                    var newGeneralLedgerAccount = new Staging.GeneralLedgerAccount()
                    {
                        ReferenceCode = generalLedgerAccount.ReferenceCode,
                        SortCode = generalLedgerAccount.SortCode,
                        ReferenceNumber = generalLedgerAccount.ReferenceNumber,
                        Name = generalLedgerAccount.Name,
                        Description = generalLedgerAccount.Description,
                        GeneralLedgerAccountType = latestNiveau2AccountType.Description,
                        GeneralLedgerAccountClassification = latestNiveau3AccountClassification.ReferenceCode,
                        CounterPartAccount = generalLedgerAccount.CounterPartAccount,
                        Parent = latestNiveau3AccountClassification.ReferenceCode,
                        BalanceSide = generalLedgerAccount.BalanceSide,
                        BalanceType = generalLedgerAccount.ReferenceCode[0].ToString(),
                        RgsLevel = generalLedgerAccount.Level,
                        IsRgsExcluded = generalLedgerAccount.IsRgsExcluded,
                        IsRgsBase = generalLedgerAccount.IsRgsBase,
                        IsRgsExtended = generalLedgerAccount.IsRgsExtended,
                        IsRgsUseWithEZ = generalLedgerAccount.IsRgsUseWithEZ,
                        IsRgsUseWithZzp = generalLedgerAccount.IsRgsUseWithZzp,
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

                    generalLedgerAccountsList.Add(newGeneralLedgerAccount);
                }

            }

            generalLedgerAccounts = generalLedgerAccountsList.ToArray();
            generalLedgerAccountClassifications = generalLedgerAccountClassificationsList.ToArray();
            generalLedgerAccountTypes = generalLedgerAccountTypesList.ToArray();
        }
    }
}
