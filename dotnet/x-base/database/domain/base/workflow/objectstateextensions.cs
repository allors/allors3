// <copyright file="TransitionalExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class ObjectStateExtensions
    {
        public static void CoreOnPostBuild(this ObjectState @this, ObjectOnPostBuild method) => @this.ObjectRevocation ??= new RevocationBuilder(@this.Transaction()).Build();
    }
}
