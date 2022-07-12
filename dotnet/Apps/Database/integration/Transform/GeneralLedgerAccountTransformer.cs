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
using Allors.Integration.Staging;
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
            this.Logger = loggerFactory.CreateLogger<GeneralLedgerAccount>();
        }

        public Source.Source Source { get; }

        public Population Population { get; }

        public ILoggerFactory LoggerFactory { get; }

        public ILogger<GeneralLedgerAccount> Logger { get; set; }

        public void Execute(out BalanceSide[] balanceSides)
        {
            var transaction = this.Population.Transaction;
            
            balanceSides = this.Source.GeneralLedgerAccounts
                .Select(v => v.BalanceSide.Equals("Debit") ?
                new BalanceSides(transaction).Debit :
                new BalanceSides(transaction).Credit
                ).ToArray();
        }
    }
}
