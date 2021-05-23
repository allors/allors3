// <copyright file="Part.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("894BE589-D536-4FEB-8B94-8E127A170F80")]
    #endregion
    public partial interface Part : UnifiedProduct
    {
        #region Allors
        [Id("bc2b9253-868b-45a2-97d7-50f671b3d857")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the Default Facility where this Part is stored.
        /// </summary>
        #region Allors
        [Id("23EC834E-849F-4CEF-9E22-BE73CCEC18FF")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Required]
        Facility DefaultFacility { get; set; }

        #region Allors
        [Id("527c0d02-7723-4715-b975-ec9474d0d22d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        PartSpecification[] PartSpecifications { get; set; }

        #region Allors
        [Id("5f727bd9-9c3e-421e-93eb-646c4fdf73d3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party ManufacturedBy { get; set; }

        #region Allors
        [Id("50C3BAB5-9BB9-48C0-B41A-9E9072D70C06")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Derived]
        Party[] SuppliedBy { get; set; }

        #region Allors
        [Id("B615880B-DA81-4437-A59B-F6350A812249")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Brand Brand { get; set; }

        #region Allors
        [Id("DCDC68FD-B69B-4320-8224-0B304EBDD62C")]
        #endregion
        [Workspace(Default)]
        [Size(10)]
        string HsCode { get; set; }

        #region Allors
        [Id("B6EB8A17-3092-44F0-86D1-59162208D5B9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Model Model { get; set; }

        #region Allors
        [Id("8dc701e0-1f66-44ee-acc6-9726aa7d5853")]
        #endregion
        [Workspace(Default)]
        int ReorderLevel { get; set; }

        #region Allors
        [Id("a093c852-cba8-43ff-9572-fd8c6cd53638")]
        #endregion
        [Workspace(Default)]
        int ReorderQuantity { get; set; }

        #region Allors
        [Id("f2c3407e-ab62-4f3e-94e5-7e9e65b89d6e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        InventoryItemKind InventoryItemKind { get; set; }

        #region Allors
        [Id("B316EB62-A654-4429-9699-403B23DB5284")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        ProductType ProductType { get; set; }

        #region Allors
        [Id("CA9F9403-B31F-4A44-9019-86272E21C1D8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        SerialisedItem[] SerialisedItems { get; set; }

        #region Allors
        [Id("ACD0DFBF-030B-410B-9A7B-E04CC748EA2D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        SerialisedItemCharacteristic[] SerialisedItemCharacteristics { get; set; }

        #region Allors
        [Id("d55ec9da-f499-47e1-9582-094e73bef11a")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        PartWeightedAverage PartWeightedAverage { get; set; }

        #region Allors
        [Id("30C81CF6-6295-44C4-ACDD-2A408DA3DC6D")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal QuantityOnHand { get; set; }

        #region Allors
        [Id("04cd1e20-a031-4a4f-9f40-6debb52b002c")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal AvailableToPromise { get; set; }

        #region Allors
        [Id("75CC0426-6695-4930-BB16-4B8B8618D7C8")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal QuantityCommittedOut { get; set; }

        #region Allors
        [Id("2ED8E0B8-3ABA-4CDE-93C7-E45AFB381E66")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal QuantityExpectedIn { get; set; }

        #region Allors

        [Id("d18b7185-0a7c-42a1-b135-0a8f7d459347")]
        #endregion
        public void SetDisplayName()
        {
        }
    }
}
