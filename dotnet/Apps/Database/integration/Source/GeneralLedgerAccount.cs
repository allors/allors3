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
    public partial class GeneralLedgerAccount
    {
        public string ExternalPrimaryKey => this.ReferenceCode;

        public string ReferenceCode { get; set; } // ex. Blva

        public string SortCode { get; set; } // ex. A.A.A010

        public string ReferenceNumber { get; set; } // ex. 0101010.01

        public string Name { get; set; } // ex. Investeringen

        public string Description { get; set; } // ex. Investeringen kosten van oprichting en van uitgifte van aandelen

        public string BalanceSide { get; set; } // ex. Debit or Credit

        public int Level { get; set; } // ex. 1 -> 5

        public bool IsRgsUseWithZzp { get; set; } // ex. 1 or Blank

        public bool IsRgsBase { get; set; } // ex. 1 or Blank

        public bool IsRgsExtended { get; set; } // ex. 1 or Blank

        public bool IsRgsUseWithEZ { get; set; } // ex. 1 or Blank

        public bool IsRgsUseWithWoco { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsBB { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsAgro { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsWKR { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsEZVOF { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsBV { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsWoco { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsBank { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsOZW { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsAfrekSyst { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsNivo5 { get; set; } // ex. 1 or Blank

        public bool ExcludeRgsUitbr5 { get; set; } // ex. 1 or Blank

        public override string ToString() => this.ReferenceCode;
    }
}
