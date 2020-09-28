// <copyright file="CostCenter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("2ab70094-5481-4ecc-ae15-cb2131fbc2f1")]
    #endregion
    public partial class CostCenter : UniquelyIdentifiable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("2a2125fd-c715-4a0f-8c1a-c1207f02a494")]
        #endregion
        [Size(-1)]
        [Workspace]
        public string Description { get; set; }

        #region Allors
        [Id("76947134-0cae-4244-a8f3-fbb018930fd3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public OrganisationGlAccount InternalTransferGlAccount { get; set; }

        #region Allors
        [Id("83a7ca20-8a73-4f8e-9729-731d25f70313")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public CostCenterCategory[] CostCenterCategories { get; set; }

        #region Allors
        [Id("975003f1-203e-4cbe-97d2-7f6ccc95f75a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public OrganisationGlAccount RedistributedCostsGlAccount { get; set; }

        #region Allors
        [Id("a3168a59-38ea-4359-b258-c9cbd656ce35")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace]
        public string Name { get; set; }

        #region Allors
        [Id("d7e01e38-d271-4c9c-847e-d26d9d4957af")]
        #endregion
        [Workspace]
        public bool Active { get; set; }

        #region Allors
        [Id("e6332140-65e7-4475-aea1-a80424640696")]
        #endregion
        [Workspace]
        public bool UseGlAccountOfBooking { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
