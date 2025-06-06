// <copyright file="InventoryItemKind.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class InventoryItemKind
    {
        public bool IsSerialised => this.Equals(new InventoryItemKinds(this.Strategy.Transaction).Serialised);

        public bool IsNonSerialised => this.Equals(new InventoryItemKinds(this.Strategy.Transaction).NonSerialised);
    }
}
