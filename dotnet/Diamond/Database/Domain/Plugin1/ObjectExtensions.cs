// <copyright file="ObjectExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hook of Plugin1 on every object: it records that it ran, through the log of the plug-in.
    public static partial class ObjectExtensions
    {
        public static void Plugin1OnPostBuild(this Object @this, ObjectOnPostBuild method) =>
            @this.Transaction().Database.Services.Get<IPlugin1Log>().Record("Plugin1", "Object.OnPostBuild", @this);
    }
}
