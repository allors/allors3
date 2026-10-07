// <copyright file="ObjectExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hook of Level1 on every object: it records that it ran. Allors binds it by name,
    // {Domain}{Method}, and runs the hooks of the domains in the order of the domains.
    public static partial class ObjectExtensions
    {
        public static void Level1OnPostBuild(this Object @this, ObjectOnPostBuild method) =>
            @this.Transaction().Database.Services.Get<ILevel1Log>().Record("Level1", "Object.OnPostBuild", @this);
    }
}
