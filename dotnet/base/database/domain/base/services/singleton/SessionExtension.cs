// <copyright file="TransactionExtension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class TransactionExtension
    {
        public static Singleton GetSingleton(this ITransaction @this)
        {
            var singletonId = @this.Database.Services.Get<ISingletonId>();

            var singleton = (Singleton)@this.Instantiate(singletonId.Id);
            if (singleton == null)
            {
                singleton = new Singletons(@this).Extent().First;
                singletonId.Id = singleton?.Id ?? 0;
            }

            return singleton;
        }
    }
}
