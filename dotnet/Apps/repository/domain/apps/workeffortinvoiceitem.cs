// <copyright file="WorkEffortInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("ced1abea-5c0f-48e3-9863-3f5d74a9cd60")]
    #endregion
    public partial class WorkEffortInvoiceItem: DelegatedAccessObject, Deletable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Object DelegatedAccess { get; set; }

        #endregion

        #region Allors
        [Id("f53f5be8-d9a9-43b5-8f63-94d44800dfbf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InvoiceItemType InvoiceItemType { get; set; }

        #region Allors
        [Id("605925e0-0658-43f8-a239-933823a1205b")]
        #endregion
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        public decimal AssignedAmount { get; set; }

        #region Allors
        [Id("94ae1041-341a-4638-90b8-501ccb802a9f")]
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(4)]
        [Workspace(Default)]
        public decimal Amount { get; set; }

        #region Allors
        [Id("6f164a59-0a88-40db-b6f8-37113e1c012f")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("72bdb2c5-3acb-4dab-a019-cb8a43f3b1e2")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [MediaType("text/markdown")]
        public string Comment { get; set; }

        #region Allors
        [Id("2fa7df40-77e8-4a05-b2de-934c4750f8d8")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Size(-1)]
        [Workspace(Default)]
        public string InternalComment { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        

        #endregion
    }
}
