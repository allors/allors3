// <copyright file="InventoryItemVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("F3D6AC19-E987-4C01-B582-A4567B7818A9")]
    #endregion
    public partial interface InventoryItemVersion : Version, Deletable
    {
        #region Allors
        [Id("E18AD324-B38C-4603-A1A0-30D7150F5FE6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Part Part { get; set; }

        #region Allors
        [Id("86010471-E2F6-4F60-AD2D-0665644CE1F6")]
        #endregion
        [Derived]
        [Size(256)]
        [Workspace(Default)]
        string Name { get; set; }

        #region Allors
        [Id("8885CCDE-B630-450C-9B01-3D18BCAA3795")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Lot Lot { get; set; }

        #region Allors
        [Id("CF0C6FDA-8E01-47B8-9787-FB0528285877")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("876D12FC-D379-4FAD-B6A6-2E7BE0EB22ED")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        Facility Facility { get; set; }
    }
}
