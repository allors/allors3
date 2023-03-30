// <copyright file="Brand.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Brand
    {
        public bool IsDeletable => !this.ExistPartsWhereBrand;

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var deletable in this.Models)
                {
                    deletable.CascadingDelete();
                }
            }
        }
    }
}
