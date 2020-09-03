// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static class DatabaseExtensions
    {
        public static void RegisterCoarseDerivations(this @IDatabase @this)
        {
            var derivations = new IDomainDerivation[]
            {
                // Core
                new AuditableDerivation(),
                new MediaDerivation(),

                // Custom
                new CoarseDerivation(),
            };

            foreach (var derivation in derivations)
            {
                @this.DomainDerivationById.Add(derivation.Id, derivation);
            }
        }


        public static void RegisterFineDerivations(this @IDatabase @this)
        {
            var derivations = new IDomainDerivation[]
            {
                // Core
                new AuditableDerivation(),
                new MediaDerivation(),

                // Custom
                new ScoreboardDerivation(),
                new GameDerivation(),
                new StartEndDateDerivation(),
                new GameDefenderDerivation(),
                new ScoreDerivation(),
                new AccumulatedScoreDerivation(),
            };

            foreach (var derivation in derivations)
            {
                @this.DomainDerivationById.Add(derivation.Id, derivation);
            }
        }
    }
}
