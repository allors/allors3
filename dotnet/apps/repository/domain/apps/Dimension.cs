// <copyright file="Dimension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("E30A6B10-069B-45CB-9D74-4DA9E77DE465")]
    #endregion
    public partial class Dimension : Enumeration
    {
        #region inherited properties

        public LocalisedText[] LocalisedNames { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Guid UniqueId { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("FF77D3D9-E425-4261-944C-1B0EC6C61B68")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public IUnitOfMeasure UnitOfMeasure { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

        #region Allors
        [Id("0F9165C3-32FE-48C0-A62E-8277592314B9")]
        #endregion
        [Workspace(Default)]
        public void Delete() { }
    }
}
