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

using System.Collections;
using Allors.Database.Domain;

namespace Allors.Integration.Staging
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class Staging
    {
        //public Dictionary<string, string> ExternalPrimaryKeyBySupplierName => this.Organisations
        //    .Where(v => v.IsSupplier)
        //    .ToDictionary(v => v.Name, v => v.ExternalPrimaryKey);

        public PostalAddress[] PostalAddresses { get; set; }

        public Database.Domain.Person[] People { get; set; }

        public Organisation[] Organisations { get; set; }
        
        public BalanceSide[] BalanceSides { get; set; }

    }
}
