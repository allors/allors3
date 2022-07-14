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
    public partial class GeneralLedgerAccountTypeLoader : Loader
    {
        public GeneralLedgerAccountTypeLoader(Staging.Staging staging, Population population, ILoggerFactory loggerFactory)
            : base(staging, population)
        {
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccountTypeLoader>();
        }

        public ILogger<GeneralLedgerAccountTypeLoader> Logger { get; set; }

        // TODO: ExternalPrimaryKey? (How to match on change?)

        public override void OnBuild()
        {
            var generalLedgerAccountTypesByDescription = this.Population.GeneralLedgerAccountTypesByDescription;
            foreach (var generalLedgerAccountType in this.Staging.GeneralLedgerAccountTypes.Where(v => !generalLedgerAccountTypesByDescription.ContainsKey(v.Description)))
            {
                new GeneralLedgerAccountTypeBuilder(this.Transaction)
                    .WithDescription(generalLedgerAccountType.Description)
                    .Build();
            }
        }

        public override void OnUpdate()
        {
            var generalLedgerAccountTypesByDescription = this.Population.GeneralLedgerAccountTypesByDescription;
            foreach (var generalLedgerAccountType in this.Staging.GeneralLedgerAccountTypes)
            {
                var generalLedgerAccountTypeToUpdate = generalLedgerAccountTypesByDescription[generalLedgerAccountType.Description];

                generalLedgerAccountTypeToUpdate.Description = generalLedgerAccountType.Description;
            }
        }
    }
}
