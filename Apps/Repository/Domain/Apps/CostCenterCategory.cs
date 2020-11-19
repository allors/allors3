// <copyright file="CostCenterCategory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("11214660-3c3a-42e9-8f12-f475d823da64")]
    #endregion
    public partial class CostCenterCategory : UniquelyIdentifiable
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("15eade6f-f540-4916-9d66-30f4bd0f260a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public CostCenterCategory Parent { get; set; }

        #region Allors
        [Id("45b0b049-e047-4490-9dde-c48fb1e7bfc3")]
        #endregion

        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public CostCenterCategory[] Ancestors { get; set; }

        #region Allors
        [Id("b20dc3d5-5067-4697-becf-0e8d44f117c7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public CostCenterCategory[] Children { get; set; }

        #region Allors
        [Id("fcb56761-342b-4d62-ba5b-27e0a0f405dd")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

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
