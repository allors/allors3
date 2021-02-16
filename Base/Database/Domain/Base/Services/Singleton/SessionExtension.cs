// <copyright file="TransactionExtension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Domain;

    public static partial class TransactionExtension
    {
        public static Singleton GetSingleton(this ITransaction @this)
        {
            var singletonService = @this.Database.Context().SingletonId;

            var singleton = (Singleton)@this.Instantiate(singletonService.Id);
            if (singleton == null)
            {
                singleton = new Singletons(@this).Extent().First;
                singletonService.Id = singleton?.Id ?? 0;
            }

            return singleton;
        }
    }
}
