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
using Allors.Database.Meta;
using Microsoft.Extensions.Logging;

namespace Allors.Integration.Transform
{
    public partial class Transform
    {
        public Transform(Source.Source source, Population population, ILoggerFactory loggerFactory)
        {
            this.LoggerFactory = loggerFactory;
            this.Source = source;
            this.Population = population;
        }

        public Source.Source Source { get; }

        public Population Population { get; }

        public ILoggerFactory LoggerFactory { get; }

        public Staging.Staging Execute()
        {
            var generalLedgerAccountTransformer = new GeneralLedgerAccountTransformer(this.Source, this.Population, this.LoggerFactory);
            generalLedgerAccountTransformer.Execute(out var generalLedgerAccounts);

            return new Staging.Staging
            {
                GeneralLedgerAccounts = generalLedgerAccounts
            };
        }
    }
}
