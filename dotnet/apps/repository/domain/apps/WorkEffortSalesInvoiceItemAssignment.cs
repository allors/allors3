// <copyright file="WorkEffortSalesInvoiceItemAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("f3f761ef-1081-4376-98d9-20d4730e3d58")]
    #endregion
    public partial class WorkEffortSalesInvoiceItemAssignment : Deletable, DelegatedAccessControlledObject
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("7197c265-151c-49e4-86b8-a27189f3c223")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public SalesInvoiceItem SalesInvoiceItem { get; set; }

        #region Allors
        [Id("b36ea9c8-c20a-4d4a-a90e-0f4dd73e7ca5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort Assignment { get; set; }

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
    }
}
