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
                // Custom
                new ClassCreationDerivation(m),
                new InterfaceCreationDerivation(m),
                new PersonFullNameDerivation(m),
                new PersonGreetingDerivation(m),
                // Validation
                new CreationDerivation(m),
                new RoleOne2OneDerivation(m),
                new RoleOne2ManyDerivation(m),
                new RoleMany2OneDerivation(m),
                new RoleMany2ManyDerivation(m),
            };

            foreach (var derivation in derivations)
            {
                @this.DomainDerivationById.Add(derivation.Id, derivation);
            }
        }
    }
}
