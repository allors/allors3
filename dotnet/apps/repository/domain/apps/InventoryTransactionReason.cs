// <copyright file="InventoryTransactionReason.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("8ff46109-8ae7-4da5-a1f9-f19d4cf4e27e")]
    #endregion
    public partial class InventoryTransactionReason : Enumeration
    {
        #region inherited properties
        public LocalisedText[] LocalisedNames { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        /// <summary>
        /// Gets or Sets a flag to indicate if Manual Entry Is Allowed for this Transaction Reason.
        /// </summary>
        #region Allors
        [Id("2CC54ADD-BB3C-4AE9-8970-917D84EC368F")]
        #endregion
        [Indexed]
        [Required]
        [Workspace(Default)]
        public bool IsManualEntryAllowed { get; set; }

        /// <summary>
        /// Gets or Sets a flag to indicate if this InventoryTransactionReason IncreasesQuantityCommittedOut for InventoryItem objects.
        /// True values increase inventory, False values decrease inventory, and null values do not affect inventory.
        /// </summary>
        #region Allors
        [Id("8F42A67D-7951-4450-8D31-7A4CBE864656")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public bool IncreasesQuantityCommittedOut { get; set; }

        /// <summary>
        /// Gets or Sets a flag to indicate if this InventoryTransactionReason IncreasesQuantityExpectedIn for InventoryItem objects.
        /// True values increase inventory, False values decrease inventory, and null values do not affect inventory.
        /// </summary>
        #region Allors
        [Id("15D50828-0A4B-4589-914F-85EE9D7D13A3")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public bool IncreasesQuantityExpectedIn { get; set; }

        /// <summary>
        /// Gets or Sets a flag to indicate if this InventoryTransactionReason IncreasesQuantityOnHand for InventoryItem objects.
        /// True values increase inventory, False values decrease inventory, and null values do not affect inventory.
        /// </summary>
        #region Allors
        [Id("C7AD0CE1-D5D4-4E2A-9E36-006BBB4E82AA")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public bool IncreasesQuantityOnHand { get; set; }

        /// <summary>
        /// Gets or Sets the Default NonSerialisedInventoryItemState which corresponds to this InventoryTransactionReason.
        /// </summary>
        #region Allors
        [Id("AE9F412A-EF95-4389-BEC2-919809BB5576")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public NonSerialisedInventoryItemState DefaultNonSerialisedInventoryItemState { get; set; }

        /// <summary>
        /// Gets or Sets the Default SerialisedInventoryItemState which corresponds to this InventoryTransactionReason.
        /// </summary>
        #region Allors
        [Id("D9B698C9-E5EC-4E1E-88AB-C8E3672835FF")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public SerialisedInventoryItemState DefaultSerialisedInventoryItemState { get; set; }

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

        #region Allors
        [Id("DC221B1A-893D-40A0-9088-2D8422593F11")]
        #endregion
        [Workspace(Default)]
        public void Delete() { }
    }
}
