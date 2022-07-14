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
        public Extract(StreamReader generalLedgerAccountList, ILoggerFactory loggerFactory)
        {
            this.GeneralLedgerAccountList = generalLedgerAccountList;
            this.LoggerFactory = loggerFactory;
            this.Logger = loggerFactory.CreateLogger<Extract>();
        }

        public StreamReader GeneralLedgerAccountList { get; }

        public ILoggerFactory LoggerFactory { get; }

        public ILogger<Extract> Logger { get; set; }

        public Source.Source Execute()
        {
            var generalLedgerAccountExtractor = new GeneralLedgerAccountExtractor(this.GeneralLedgerAccountList, this.LoggerFactory);

            return new Source.Source
            {
                GeneralLedgerAccounts = generalLedgerAccountExtractor.Execute(),
            };
        }
    }
}
