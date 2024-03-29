// <copyright file="PickListItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PickListItem
    {
        public void AppsDelete(PickListItemDelete method)
        {
            foreach (var itemIssuance in this.ItemIssuancesWherePickListItem)
            {
                itemIssuance.CascadingDelete();
            }
        }
    }
}
