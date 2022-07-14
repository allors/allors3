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

namespace Allors.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database;
    using Allors.Database.Domain;
    //using Allors.Database.Meta;

    public partial class Population
    {
        public ITransaction Transaction { get; set; }

        public Dictionary<string, GeneralLedgerAccount> GeneralLedgerAccountsByExternalPrimaryKey => new GeneralLedgerAccounts(this.Transaction).Extent().ToDictionary(v => v.ExternalPrimaryKey);

        public Dictionary<string, GeneralLedgerAccountType> GeneralLedgerAccountTypesByDescription => new GeneralLedgerAccountTypes(this.Transaction).Extent().ToDictionary(v => v.Description);

        public Dictionary<string, GeneralLedgerAccountClassification> GeneralLedgerAccountClassificationsByExternalPrimaryKey => new GeneralLedgerAccountClassifications(this.Transaction).Extent().ToDictionary(v => v.ExternalPrimaryKey);

        public Dictionary<string, BalanceSide> BalanceSideByName => new BalanceSides(this.Transaction).Extent().ToDictionary(v => v.Name[0].ToString());

        public Dictionary<string, BalanceType> BalanceTypesByName => new BalanceTypes(this.Transaction).Extent().ToDictionary(v => v.Name[0].ToString());
    }
}
