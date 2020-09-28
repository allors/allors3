// <copyright file="InventoryStrategy.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;

    #region Allors
    [Id("5E5FE517-4AF6-4977-9E15-8D377E518F03")]
    #endregion
    public partial class InventoryStrategy : UniquelyIdentifiable
    {
        #region inheritedProperties
        public Guid UniqueId { get; set; }
        #endregion inheritedProperties

        #region Allors
        [Id("87EAE8DA-47CF-405E-BFA0-799C87284CC9")]
        #endregion
        [Size(256)]
        [Workspace]
        public string Name { get; set; }

        /* SerialisedInventoryItemState InventoryStrategy Items
         ******************************************************/

        /// <summary>
        /// The SerialisedInventoryItemStates included in AvailableToPromise calculations for this InventoryStrategy.
        /// </summary>
        #region Allors
        [Id("6E36E878-B821-4A74-B722-B834E8204D18")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public SerialisedInventoryItemState[] AvailableToPromiseSerialisedStates { get; set; }

        /// <summary>
        /// The SerialisedInventoryItemStates included in QuantityOnHand calculations for this InventoryStrategy.
        /// </summary>
        #region Allors
        [Id("9D87E54C-5EB5-4014-84E8-9957126430CA")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public SerialisedInventoryItemState[] OnHandSerialisedStates { get; set; }

        /* NonSerialisedInventoryItemState InventoryStrategy Items
         *********************************************************/

        /// <summary>
        /// The NonSerialisedInventoryItemStates included in AvailableToPromise calculations for this InventoryStrategy.
        /// </summary>
        #region Allors
        [Id("2F90BA87-BEEC-4BB6-BF49-45A622B22BD4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public NonSerialisedInventoryItemState[] AvailableToPromiseNonSerialisedStates { get; set; }

        /// <summary>
        /// The NonSerialisedInventoryItemStates included in QuantityOnHand calculations for this InventoryStrategy.
        /// </summary>
        #region Allors
        [Id("F20911A6-B1A6-46E8-955B-4286DE54D806")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public NonSerialisedInventoryItemState[] OnHandNonSerialisedStates { get; set; }

        #region inheritedMethods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnDerive() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void OnPreDerive() { }
        #endregion inheritedMethods
    }
}
