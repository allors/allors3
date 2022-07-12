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

namespace Allors.Integration.Staging
{
    using Allors.Database.Domain;

    public partial class GeneralLedgerAccount
    {
        public string ExternalPrimaryKey { get; set; }

        public string ReferenceCode { get; set; }

        public string SortCode { get; set; }

        public string ReferenceNumber { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        //public string SearchCode { get; set; }

        public string GeneralLedgerAccountType { get; set; }

        //GeneralLedgerAccountClassification?

        public string CounterPartAccount { get; set; }

        public string Parent { get; set; }

        //CashAccount

        //CostCenterAccount

        public string BalanceSide { get; set; }

        public string BalanceType { get; set; }

        public int RgsLevel { get; set; }

        public bool IsRgsUseWithZzp { get; set; }

        public bool IsRgsBase { get; set; }

        public bool IsRgsExtended { get; set; }

        public bool IsRgsUseWithEZ { get; set; }

        public bool IsRgsUseWithWoco { get; set; }
            
        public bool ExcludeRgsBB { get; set; }

        public bool ExcludeRgsAgro { get; set; }

        public bool ExcludeRgsWKR { get; set; }

        public bool ExcludeRgsEZVOF { get; set; }

        public bool ExcludeRgsBV { get; set; }

        public bool ExcludeRgsWoco { get; set; }

        public bool ExcludeRgsBank { get; set; }

        public bool ExcludeRgsOZW { get; set; }

        public bool ExcludeRgsAfrekSyst { get; set; }

        public bool ExcludeRgsNivo5 { get; set; }

        public bool ExcludeRgsUitbr5 { get; set; }
    }
}
