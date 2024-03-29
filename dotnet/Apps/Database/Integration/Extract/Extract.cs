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

namespace Allors.Integration.Extract
{
    using System.IO;
    using global::Integration.Extract;
    using Microsoft.Extensions.Logging;

    public partial class Extract
    {

        public Extract(StreamReader generalLedgerAccountList, StreamReader marAccountingObligeeEnterprisesGeneralLedgerAccountList, StreamReader marAssociationsAndFoundationsGeneralLedgerAccountList, ILoggerFactory loggerFactory)
        {
            this.GeneralLedgerAccountList = generalLedgerAccountList;
            this.MarAccountingObligeeEnterprisesGeneralLedgerAccountList = marAccountingObligeeEnterprisesGeneralLedgerAccountList;
            this.MarAssociationsAndFoundationsGeneralLedgerAccountList = marAssociationsAndFoundationsGeneralLedgerAccountList;
            this.LoggerFactory = loggerFactory;

        }

        public StreamReader GeneralLedgerAccountList { get; }

        public StreamReader MarAccountingObligeeEnterprisesGeneralLedgerAccountList { get; }
        public StreamReader MarAssociationsAndFoundationsGeneralLedgerAccountList { get; }

        public ILoggerFactory LoggerFactory { get; }

        public ILogger<Extract> Logger { get; set; }

        public Source.Source Execute()
        {
            var generalLedgerAccountExtractor = new GeneralLedgerAccountExtractor(this.GeneralLedgerAccountList, this.LoggerFactory);
            var marAccountingObligeeEnterprisesGeneralLedgerAccountExtractor = new MarAccountingObligeeEnterprisesGeneralLedgerAccountExtractor(this.MarAccountingObligeeEnterprisesGeneralLedgerAccountList, this.LoggerFactory);
            var marAssociationsAndFoundationsGeneralLedgerAccountExtractor = new MarAssociationsAndFoundationsGeneralLedgerAccountExtractor(this.MarAssociationsAndFoundationsGeneralLedgerAccountList, this.LoggerFactory);

            return new Source.Source
            {
                GeneralLedgerAccounts = generalLedgerAccountExtractor.Execute(),
                MarAccountingObligeeEnterprisesGeneralLedgerAccounts = marAccountingObligeeEnterprisesGeneralLedgerAccountExtractor.Execute(),
                MarAssociationsAndFoundationsGeneralLedgerAccounts = marAssociationsAndFoundationsGeneralLedgerAccountExtractor.Execute(),
            };
        }
    }
}
