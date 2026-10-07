// <copyright file="ObjectExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hook of the concrete domain on every object: it records that it ran. The concrete domain
    // knows both logs; it uses the one of Level1.
    public static partial class ObjectExtensions
    {
        public static void TestOnPostBuild(this Object @this, ObjectOnPostBuild method) =>
            @this.Transaction().Database.Services.Get<ILevel1Log>().Record("Test", "Object.OnPostBuild", @this);
    }
}
