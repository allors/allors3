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
            var m = @this.State().M;
            var derivations = new IDomainDerivation[]
            {
            };

            foreach (var derivation in derivations)
            {
                @this.AddDerivation(derivation);
            }
        }

        public static void RegisterCoarseDerivations(this @IDatabase @this)
        {
            var m = @this.State().M;
            var derivations = new IDomainDerivation[]
            {
                new CoarseDerivation(m),
            };

            foreach (var derivation in derivations)
            {
                @this.AddDerivation(derivation);
            }
        }

        public static void RegisterFineDerivations(this @IDatabase @this)
        {
            var m = @this.State().M;
            var derivations = new IDomainDerivation[]
            {
                new ScoreboardDerivation(m),
                new GameDerivation(m),
                new StartEndDateDerivation(m),
                new GameDefenderDerivation(m),
                new ScoreDerivation(m),
                new AccumulatedScoreDerivation(m),
            };

            foreach (var derivation in derivations)
            {
                @this.AddDerivation(derivation);
            }
        }
    }
}
