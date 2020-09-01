// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static class DatabaseExtensions
    {
        public static void RegisterDerivations(this @IDatabase @this)
        {
            var derivations = new IDomainDerivation[]
            {
                new PersonFullNameDerivation(),
                new PersonGreetingDerivation(),
                new AuditableDerivation(),
                new MediaDerivation(),
            };

            foreach (var derivation in derivations)
            {
                @this.DomainDerivationById[derivation.Id] = derivation;
            }
        }
    }
}
