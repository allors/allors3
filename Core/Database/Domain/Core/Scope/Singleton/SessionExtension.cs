// <copyright file="SessionExtension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static partial class SessionExtension
    {
        public static Singleton GetSingleton(this ISession @this) => (Singleton)@this.Instantiate(@this.Database.Scope().SingletonService.Id);
    }
}
