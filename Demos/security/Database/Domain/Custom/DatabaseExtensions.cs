// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Domain;

    public static partial class DatabaseExtensions
    {
        public static void RegisterDerivations(this @IDatabase @this)
        {
            var m = @this.State().M;
            var derivations = new IDomainDerivation[]
            {
            };

            foreach (var derivation in derivations)
            {
                @this.AddDerivation(derivation);
            }
        }
    }
}
