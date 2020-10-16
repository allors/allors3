// <copyright file="InventoryItemConfiguration.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("f135770b-7228-4e4b-b7ea-9307b6317fd2")]
    #endregion
    public partial interface InventoryItemConfiguration : Commentable, Object
    {
        #region Allors
        [Id("92a85a6b-4f65-4ba4-bd5e-bf44d5a9ca56")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        InventoryItem InventoryItem { get; set; }

        #region Allors
        [Id("f041b297-e2bb-4ada-ab89-08ec9bcd6513")]
        #endregion
        [Required]
        int Quantity { get; set; }

        #region Allors
        [Id("f1d4ceeb-f859-4996-babc-dc55837489e0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        InventoryItem ComponentInventoryItem { get; set; }
    }
}
