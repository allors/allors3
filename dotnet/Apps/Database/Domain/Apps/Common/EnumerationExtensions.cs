// <copyright file="EnumerationExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class EnumerationExtensions
    {
        public static void AppsOnBuild(this Enumeration @this, ObjectOnBuild method)
        {
            if (!@this.IsActive.HasValue)
            {
                @this.IsActive = true;
            }
        }
    }
}
