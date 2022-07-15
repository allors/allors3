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
    public partial class GeneralLedgerAccountClassificationLoader : Loader
    {
        public GeneralLedgerAccountClassificationLoader(Staging.Staging staging, Population population, ILoggerFactory loggerFactory)
            : base(staging, population)
        {
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccountClassificationLoader>();
        }

        public ILogger<GeneralLedgerAccountClassificationLoader> Logger { get; set; }

        public override void OnBuild()
        {
            var generalLedgerAccountClassificationsByExternalPrimaryKey = this.Population.GeneralLedgerAccountClassificationsByExternalPrimaryKey;
            foreach (var generalLedgerAccountClassification in this.Staging.GeneralLedgerAccountClassifications.Where(v => !generalLedgerAccountClassificationsByExternalPrimaryKey.ContainsKey(v.ExternalPrimaryKey)))
            {
                new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                    .WithExternalPrimaryKey(generalLedgerAccountClassification.ExternalPrimaryKey)
                    .Build();
            }
        }

        public override void OnUpdate()
        {
            var generalLedgerAccountClassificationsByExternalPrimaryKey = this.Population.GeneralLedgerAccountClassificationsByExternalPrimaryKey;
            foreach (var generalLedgerAccountClassification in this.Staging.GeneralLedgerAccountClassifications)
            {
                var generalLedgerAccountClassificationToUpdate = generalLedgerAccountClassificationsByExternalPrimaryKey[generalLedgerAccountClassification.ExternalPrimaryKey];

                if(generalLedgerAccountClassification.ParentExternalPrimaryKey != null)
                {
                    generalLedgerAccountClassificationToUpdate.Parent = generalLedgerAccountClassificationsByExternalPrimaryKey[generalLedgerAccountClassification.ParentExternalPrimaryKey];
                }

                generalLedgerAccountClassificationToUpdate.RgsLevel = generalLedgerAccountClassification.RgsLevel;
                generalLedgerAccountClassificationToUpdate.ReferenceCode = generalLedgerAccountClassification.ReferenceCode;
                generalLedgerAccountClassificationToUpdate.SortCode = generalLedgerAccountClassification.SortCode;
                generalLedgerAccountClassificationToUpdate.ReferenceNumber = generalLedgerAccountClassification.ReferenceNumber;
                generalLedgerAccountClassificationToUpdate.Name = generalLedgerAccountClassification.Name;
            }
        }
    }
}
