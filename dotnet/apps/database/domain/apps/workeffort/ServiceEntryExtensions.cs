// <copyright file="ServiceEntryExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public static partial class ServiceEntryExtensions
    {
        public static void AppsOnBuild(this ServiceEntry @this, ObjectOnBuild method) => @this.DerivationTrigger = Guid.NewGuid();
    }
}
