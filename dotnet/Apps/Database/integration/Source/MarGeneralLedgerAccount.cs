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

namespace Allors.Integration.Source
{
    using System;

    public partial class MarGeneralLedgerAccount
    {
        public string ExternalPrimaryKey => this.ReferenceCode;

        public string ReferenceCode { get; set; } // ex. 201

        public string Name { get; set; } // ex. Concessies, octrooien, licenties, know-how, merken en soortgelijke rechten

        public string IsActiva { get; set; } // ex. IV.A.1

        public string IsPassiva { get; set; } // ex. VIII.A.5

        public string BalanceType { get; set; } // ex. VIII.A.5	

        public string FirstCharFromReferenceCode()
        {
            if(this.ReferenceCode.Length == 0)
            {
                return "";
            }

            return this.ReferenceCode[0].ToString();
        }
        public string GetBalanceSide()
        {
            if (!string.IsNullOrWhiteSpace(this.IsActiva))
            {
                return "Debit";
            }
            else if (!string.IsNullOrWhiteSpace(this.IsPassiva))
            {
                return "Credit";
            }

            return "";
        }

        public bool IsEmpty() =>
            string.IsNullOrWhiteSpace(this.ReferenceCode) &&
            string.IsNullOrWhiteSpace(this.Name) &&
            string.IsNullOrWhiteSpace(this.IsActiva) &&
            string.IsNullOrWhiteSpace(this.IsPassiva);

        public override string ToString() => this.ReferenceCode;
    }
}
