// <copyright file="SerialisedInventoryItems.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SerialisedInventoryItems
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.SerialisedInventoryItemState);
    }
}
