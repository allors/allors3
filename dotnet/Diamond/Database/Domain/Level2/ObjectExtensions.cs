// <copyright file="ObjectExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hook of Level2 on every object: it records that it ran, through the log of Level1.
    public static partial class ObjectExtensions
    {
        public static void Level2OnPostBuild(this Object @this, ObjectOnPostBuild method) =>
            @this.Transaction().Database.Services.Get<ILevel1Log>().Record("Level2", "Object.OnPostBuild", @this);
    }
}
