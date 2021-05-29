// <copyright file="ObjectExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class PermissionExtensions
    {
        public static void CoreOnPostDerive(this Permission @this, ObjectOnPostDerive _)
            => @this.DatabaseServices().PermissionsCache.Clear();

        public static void CoreDelete(this Permission @this, DeletableDelete _)
            => @this.DatabaseServices().PermissionsCache.Clear();
    }
}
