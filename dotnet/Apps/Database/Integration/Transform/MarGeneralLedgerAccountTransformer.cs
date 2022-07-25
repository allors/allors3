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

using System;
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

        public void Execute(out Staging.MarGeneralLedgerAccount[] marGeneralLedgerAccount)
        {
            var marGeneralLedgerAccountList = new List<Staging.MarGeneralLedgerAccount>();


            var marGeneralLedgerAccountsByType = this.Source.MarGeneralLedgerAccounts.GroupBy(v => v.FirstCharFromReferenceCode());

            foreach (var marGeneralLedgerAccountsTypeGroup in marGeneralLedgerAccountsByType.Where(v => !v.Key.StartsWith(".")))
            {
                //var parentStack = new Stack<Staging.GeneralLedgerAccountClassification>();

                foreach (var generalLedgerAccount in marGeneralLedgerAccountsTypeGroup)
                {
                    var generalLedgerAccountClassification = new Staging.MarGeneralLedgerAccount()
                    {
                        ReferenceCode = generalLedgerAccount.ReferenceCode,
                        Name = generalLedgerAccount.Name.Replace(@"\((.*?)\)", ""),
                        Nivo = generalLedgerAccount.ReferenceCode.Length,
                        Activa = generalLedgerAccount.IsActiva,
                        Passiva = generalLedgerAccount.IsPassiva,
                        BalanceType = generalLedgerAccount.BalanceType
                    };

                    //if (parentStack.Count == 0)
                    //{
                    //    generalLedgerAccountClassificationsList.Add(generalLedgerAccountClassification);
                    //    parentStack.Push(generalLedgerAccountClassification);
                    //    continue;
                    //}

                    //while (generalLedgerAccountClassification.ReferenceCode.Length <= parentStack.Peek().ReferenceCode.Length)
                    //{
                    //    parentStack.Pop();
                    //}


                    //generalLedgerAccountClassification.ParentExternalPrimaryKey = parentStack.Peek().ExternalPrimaryKey;

                    marGeneralLedgerAccountList.Add(generalLedgerAccountClassification);
                    //parentStack.Push(generalLedgerAccountClassification);
                }
            }

            marGeneralLedgerAccount = marGeneralLedgerAccountList.ToArray();
        }
    }
}
