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
            foreach (var generalLedgerAccountClassification in this.Staging.GeneralLedgerAccountClassifications)
            {
                new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                    .WithRgsLevel(generalLedgerAccountClassification.RgsLevel)
                    .WithReferenceCode(generalLedgerAccountClassification.ReferenceCode)
                    .WithSortCode(generalLedgerAccountClassification.SortCode)
                    .WithCode(generalLedgerAccountClassification.Code)
                    //.WithParent()
                    .WithName(generalLedgerAccountClassification.Name)
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
