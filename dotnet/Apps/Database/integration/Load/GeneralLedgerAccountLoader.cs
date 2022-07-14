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

using System.Linq;
using Allors.Database.Domain;
using Microsoft.Extensions.Logging;

namespace Allors.Integration.Load
{
    public partial class GeneralLedgerAccountLoader : Loader
    {
        public GeneralLedgerAccountLoader(Staging.Staging staging, Population population, ILoggerFactory loggerFactory)
            : base(staging, population)
        {
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccountLoader>();
        }

        public ILogger<GeneralLedgerAccountLoader> Logger { get; set; }

        public override void OnBuild()
        {
            var generalLedgerAccountsByExternalPrimaryKey = this.Population.GeneralLedgerAccountsByExternalPrimaryKey;
            foreach (var generalLedgerAccount in this.Staging.GeneralLedgerAccounts.Where(v => !generalLedgerAccountsByExternalPrimaryKey.ContainsKey(v.ExternalPrimaryKey)))
            {
                new GeneralLedgerAccountBuilder(this.Transaction)
                    .WithExternalPrimaryKey(generalLedgerAccount.ExternalPrimaryKey)
                    .WithReferenceCode(generalLedgerAccount.ReferenceCode)
                    .WithSortCode(generalLedgerAccount.SortCode)
                    .WithReferenceNumber(generalLedgerAccount.ReferenceNumber)
                    .WithName(generalLedgerAccount.Name)
                    .WithDescription(generalLedgerAccount.Description)
                    .WithBalanceSide(this.Population.BalanceSideByName.Get(generalLedgerAccount.BalanceSide))
                    .WithBalanceType(this.Population.BalanceTypesByName.Get(generalLedgerAccount.BalanceType))
                    .WithRgsLevel(generalLedgerAccount.RgsLevel)
                    .WithIsRgsExcluded(generalLedgerAccount.IsRgsExcluded)
                    .WithIsRgsBase(generalLedgerAccount.IsRgsBase)
                    .WithIsRgsExtended(generalLedgerAccount.IsRgsExtended)
                    .WithIsRgsUseWithEZ(generalLedgerAccount.IsRgsUseWithEZ)
                    .WithIsRgsUseWithZzp(generalLedgerAccount.IsRgsUseWithZzp)
                    .WithIsRgsUseWithWoco(generalLedgerAccount.IsRgsUseWithWoco)
                    .WithExcludeRgsBB(generalLedgerAccount.ExcludeRgsBB)
                    .WithExcludeRgsAgro(generalLedgerAccount.ExcludeRgsAgro)
                    .WithExcludeRgsWKR(generalLedgerAccount.ExcludeRgsWKR)
                    .WithExcludeRgsEZVOF(generalLedgerAccount.ExcludeRgsEZVOF)
                    .WithExcludeRgsBV(generalLedgerAccount.ExcludeRgsBV)
                    .WithExcludeRgsWoco(generalLedgerAccount.ExcludeRgsWoco)
                    .WithExcludeRgsBank(generalLedgerAccount.ExcludeRgsBank)
                    .WithExcludeRgsOZW(generalLedgerAccount.ExcludeRgsOZW)
                    .WithExcludeRgsAfrekSyst(generalLedgerAccount.ExcludeRgsAfrekSyst)
                    .WithExcludeRgsNivo5(generalLedgerAccount.ExcludeRgsNivo5)
                    .WithExcludeRgsUitbr5(generalLedgerAccount.ExcludeRgsUitbr5)
                    .Build();
            }
        }

        public override void OnUpdate()
        {
            var generalLedgerAccountsByExternalPrimaryKey = this.Population.GeneralLedgerAccountsByExternalPrimaryKey;
            foreach (var generalLedgerAccount in this.Staging.GeneralLedgerAccounts)
            {
                var generalLedgerAccountToUpdate = generalLedgerAccountsByExternalPrimaryKey[generalLedgerAccount.ExternalPrimaryKey];

                generalLedgerAccountToUpdate.GeneralLedgerAccountType = this.Population.GeneralLedgerAccountTypesByDescription[generalLedgerAccount.GeneralLedgerAccountTypeDescription];
                generalLedgerAccountToUpdate.GeneralLedgerAccountClassification = this.Population.GeneralLedgerAccountClassificationsByExternalPrimaryKey[generalLedgerAccount.GeneralLedgerAccountClassificationExternalPrimaryKey];

                if (generalLedgerAccountToUpdate.CounterPartAccount != null)
                {
                    generalLedgerAccountToUpdate.CounterPartAccount = this.Population.GeneralLedgerAccountsByExternalPrimaryKey[generalLedgerAccount.CounterPartAccountExternalPrimaryKey];
                }

                generalLedgerAccountToUpdate.ReferenceCode = generalLedgerAccount.ReferenceCode;
                generalLedgerAccountToUpdate.SortCode = generalLedgerAccount.SortCode;
                generalLedgerAccountToUpdate.ReferenceNumber = generalLedgerAccount.ReferenceNumber;
                generalLedgerAccountToUpdate.Name = generalLedgerAccount.Name;
                generalLedgerAccountToUpdate.Description = generalLedgerAccount.Description;
                generalLedgerAccountToUpdate.BalanceSide = this.Population.BalanceSideByName.Get(generalLedgerAccount.BalanceSide);
                generalLedgerAccountToUpdate.BalanceType = this.Population.BalanceTypesByName.Get(generalLedgerAccount.BalanceType);
                generalLedgerAccountToUpdate.RgsLevel = generalLedgerAccount.RgsLevel;
                generalLedgerAccountToUpdate.IsRgsExcluded = generalLedgerAccount.IsRgsExcluded;
                generalLedgerAccountToUpdate.IsRgsBase = generalLedgerAccount.IsRgsBase;
                generalLedgerAccountToUpdate.IsRgsExtended = generalLedgerAccount.IsRgsExtended;
                generalLedgerAccountToUpdate.IsRgsUseWithEZ = generalLedgerAccount.IsRgsUseWithEZ;
                generalLedgerAccountToUpdate.IsRgsUseWithZzp = generalLedgerAccount.IsRgsUseWithZzp;
                generalLedgerAccountToUpdate.IsRgsUseWithWoco = generalLedgerAccount.IsRgsUseWithWoco;
                generalLedgerAccountToUpdate.ExcludeRgsBB = generalLedgerAccount.ExcludeRgsBB;
                generalLedgerAccountToUpdate.ExcludeRgsAgro = generalLedgerAccount.ExcludeRgsAgro;
                generalLedgerAccountToUpdate.ExcludeRgsWKR = generalLedgerAccount.ExcludeRgsWKR;
                generalLedgerAccountToUpdate.ExcludeRgsEZVOF = generalLedgerAccount.ExcludeRgsEZVOF;
                generalLedgerAccountToUpdate.ExcludeRgsBV = generalLedgerAccount.ExcludeRgsBV;
                generalLedgerAccountToUpdate.ExcludeRgsWoco = generalLedgerAccount.ExcludeRgsWoco;
                generalLedgerAccountToUpdate.ExcludeRgsBank = generalLedgerAccount.ExcludeRgsBank;
                generalLedgerAccountToUpdate.ExcludeRgsOZW = generalLedgerAccount.ExcludeRgsOZW;
                generalLedgerAccountToUpdate.ExcludeRgsAfrekSyst = generalLedgerAccount.ExcludeRgsAfrekSyst;
                generalLedgerAccountToUpdate.ExcludeRgsNivo5 = generalLedgerAccount.ExcludeRgsNivo5;
                generalLedgerAccountToUpdate.ExcludeRgsUitbr5 = generalLedgerAccount.ExcludeRgsUitbr5;
            }
        }
    }
}
