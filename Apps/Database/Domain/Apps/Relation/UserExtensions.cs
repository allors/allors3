// <copyright file="UserExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static partial class UserExtensions
    {
        public static void AppsDelete(this User @this, DeletableDelete method) => @this.UserProfile?.Delete();
    }
}
