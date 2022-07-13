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
            var generalLedgerAccounts = this.Staging.GeneralLedgerAccounts;
            //var balanceSides = this.Population.

            foreach (var generalLedgerAccount in generalLedgerAccounts)
            {
                new GeneralLedgerAccountBuilder(this.Transaction)
                    .WithReferenceCode(generalLedgerAccount.ReferenceCode)
                    .WithSortCode(generalLedgerAccount.SortCode)
                    .WithReferenceNumber(generalLedgerAccount.ReferenceNumber)
                    .WithName(generalLedgerAccount.Name)
                    .WithDescription(generalLedgerAccount.Description)
                    //.WithGeneralLedgerAccountType(generalLedgerAccount.GeneralLedgerAccountType)
                    //.WithCounterPartAccount()
                    //.WithParent()
                    .WithBalanceSide(this.Population.BalanceSideByName.Get(generalLedgerAccount.BalanceSide))
                    .WithBalanceType(this.Population.BalanceTypesByName.Get(generalLedgerAccount.BalanceType))
                    .WithRgsLevel(generalLedgerAccount.RgsLevel)
                    .WithIsRgsUseWithZzp(generalLedgerAccount.IsRgsUseWithZzp)
                    .WithIsRgsBase(generalLedgerAccount.IsRgsBase)
                    .WithIsRgsExtended(generalLedgerAccount.IsRgsExtended)
                    .WithIsRgsUseWithEZ(generalLedgerAccount.IsRgsUseWithEZ)
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
            //foreach (var stagingPerson in this.Staging.People)
            //{
            //var person = personByExternalPersonKey[stagingPerson.ExternalPersonKey];
            //person.FirstName = stagingPerson.FirstName;
            //person.LastName = stagingPerson.LastName;
            //person.Salutation = salutationByName.Get(stagingPerson.Salutation);
            //}
        }
    }
}
