
// <copyright file="InventoryItem.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("61af6d19-e8e4-4b5b-97e8-3610fbc82605")]
    #endregion
    public partial interface InventoryItem : UniquelyIdentifiable, Transitional, Deletable, Versioned, Searchable, IDisplayName
    {
        /// <summary>
        /// Gets or sets the Part for which this InventoryItem tracks inventory information.
        /// </summary>
        #region Allors
        [Id("BCC41DF1-D526-4C78-8F68-B32AB104AD12")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        Part Part { get; set; }

        /// <summary>
        /// Gets or sets the Facility at which this InventoryItem tracks inventory information.
        /// </summary>
        #region Allors
        [Id("BC234CEA-DC2E-4BDC-B911-5A12D1D6F354")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        Facility Facility { get; set; }

        /// <summary>
        /// Gets or sets the UnitOfMeasure which describes the inventory tracked by this Inventory Item.
        /// </summary>
        #region Allors
        [Id("D276D126-34D3-4820-884C-EC9944B5E10B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("2678441b-342c-4b94-a5c7-d8c9e07de6b4")]
        #endregion
        [Derived]
        [Origin(Origin.Session)]
        [Workspace(Default)]
        string PartDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the (optional) Lot in which this InventoryItem tracks inventory information.
        /// </summary>
        #region Allors
        [Id("8573F543-0EB9-4A5E-A68F-CC69CD5CF8F9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Lot Lot { get; set; }

        #region Workspace
        #region Allors
        [Id("ab7dfe1c-6753-4735-8090-bb782b0ab631")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        string FacilityName { get; set; }
        #endregion
    }
}
