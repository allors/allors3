// <copyright file="ObjectExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class DeletableExtensions
    {
        public static bool IsDeleting(this Deletable @this)
        {
            if (@this.Strategy.IsDeleted)
            {
                return false;
            }

            var deleting = @this.Strategy.Transaction.Services.Get<IDeleting>();
            return deleting.IsDeleting(@this.Id);
        }

        public static void CascadingDelete(this Deletable @this)
        {
            if (!@this.IsDeleting())
            {
                @this.Delete();
            }
        }
    }
}
