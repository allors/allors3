// <copyright file="WorkEffortPurchaseOrderItemAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("2C8554EF-7B0E-47A3-AC66-E6CB50E20DF9")]
    #endregion
    public partial class WorkEffortPurchaseOrderItemAssignment : Deletable, DelegatedAccessControlledObject
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("37D28124-9D29-435C-838F-7B043DCFFF33")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort Assignment { get; set; }

        #region Allors
        [Id("C314C1F1-1E49-4CC5-9915-77ABCB41D685")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public PurchaseOrderItem PurchaseOrderItem { get; set; }

        #region Allors
        [Id("95577BE1-EC7A-41D2-B51F-96791B172B84")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public PurchaseOrder PurchaseOrder { get; set; }

        #region Allors
        [Id("346A4EC3-16F7-45EB-9BD6-949EDB86BCFF")]
        #endregion
        [Required]
        [Workspace(Default)]
        public int Quantity { get; set; }

        #region Allors
        [Id("B4697583-DBD5-4713-AF01-50093D3BDD6E")]
        #endregion
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        public decimal AssignedUnitSellingPrice { get; set; }

        #region Allors
        [Id("7EFCD991-929F-4A0E-A066-36A0BD4C045D")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal UnitPurchasePrice { get; set; }

        #region Allors
        [Id("017BC7F2-A9A6-46F4-A4EE-B47F802D7BB2")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal UnitSellingPrice { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        public void DelegateAccess() { }

        #endregion

        #region Allors

        [Id("99e7aefd-9a70-4fc2-ad3d-a8606bd6ba3d")]
        #endregion
        public void CalculateSellingPrice()
        {
        }
    }
}
