// <copyright file="ServiceConfiguration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("5d4beea4-f480-460e-92ee-3e8d532ac7f9")]
    #endregion
    public partial class ServiceConfiguration : InventoryItemConfiguration
    {
        #region inherited properties
        public InventoryItem InventoryItem { get; set; }

        public int Quantity { get; set; }

        public InventoryItem ComponentInventoryItem { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
