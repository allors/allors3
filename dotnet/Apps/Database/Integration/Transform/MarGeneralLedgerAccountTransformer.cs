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
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Allors.Integration.Transform
{
    public partial class MarGeneralLedgerAccountTransformer
    {
        public MarGeneralLedgerAccountTransformer(Source.Source source, Population population, ILoggerFactory loggerFactory)
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

        public void Execute(out Staging.GeneralLedgerAccountClassification[] generalLedgerAccountClassifications, out Staging.GeneralLedgerAccountType[] generalLedgerAccountTypes)
        {
            var generalLedgerAccountTypesList = new List<Staging.GeneralLedgerAccountType>();
            var generalLedgerAccountClassificationsList = new List<Staging.GeneralLedgerAccountClassification>();

            var marGeneralLedgerAccountsByType = this.Source.MarGeneralLedgerAccounts.GroupBy(v => v.FirstCharFromReferenceCode());

            foreach (var marGeneralLedgerAccountsTypeGroup in marGeneralLedgerAccountsByType.Where(v => !v.Key.StartsWith(".")))
            {
                var generalLedgerAccountType = new Staging.GeneralLedgerAccountType()
                {
                    Description = marGeneralLedgerAccountsTypeGroup.First().Name,
                };

                generalLedgerAccountTypesList.Add(generalLedgerAccountType);

                var parentStack = new Stack<Staging.GeneralLedgerAccountClassification>();

                foreach (var generalLedgerAccount in marGeneralLedgerAccountsTypeGroup)
                {
                    var generalLedgerAccountClassification = new Staging.GeneralLedgerAccountClassification()
                    {
                        ReferenceCode = generalLedgerAccount.ReferenceCode,
                        Name = generalLedgerAccount.Name.Replace(@"\((.*?)\)", ""),
                    };

                    if (parentStack.Count == 0)
                    {
                        generalLedgerAccountClassificationsList.Add(generalLedgerAccountClassification);
                        parentStack.Push(generalLedgerAccountClassification);
                        continue;
                    }

                    while (generalLedgerAccountClassification.ReferenceCode.Length <= parentStack.Peek().ReferenceCode.Length)
                    {
                        parentStack.Pop();
                    }

                    generalLedgerAccountClassification.ParentExternalPrimaryKey = parentStack.Peek().ExternalPrimaryKey;

                    generalLedgerAccountClassificationsList.Add(generalLedgerAccountClassification);
                    parentStack.Push(generalLedgerAccountClassification);
                }
            }

            generalLedgerAccountTypes = generalLedgerAccountTypesList.ToArray();
            generalLedgerAccountClassifications = generalLedgerAccountClassificationsList.ToArray();
        }
    }
}
