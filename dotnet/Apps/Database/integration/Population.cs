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

        public Country[] Countries => new Countries(this.Transaction).Extent().ToArray();

        //public Dictionary<string, Person> PersonByExternalPersonKey => new People(this.Transaction).Extent().Where(v => v.ExistExternalPrimaryKey).GroupBy(v => v.ExternalPersonKey).Select(v => v.First()).ToDictionary(v => v.ExternalPersonKey);

        public Dictionary<string, Salutation> SalutationByName => new Salutations(this.Transaction).Extent().Where(v => v.ExistName).ToDictionary(v => v.Name);

        public GeneralLedgerAccount[] GeneralLedgerAccounts => new GeneralLedgerAccounts(this.Transaction).Extent().ToArray();

        public Dictionary<string, BalanceSide> BalanceSideByName => new BalanceSides(this.Transaction).Extent().ToDictionary(v => v.Name[0].ToString());

        public Dictionary<string, BalanceType> BalanceTypesByName => new BalanceTypes(this.Transaction).Extent().ToDictionary(v => v.Name[0].ToString());
    }
}
