// <copyright file="PopulationMetadata.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    public sealed class PopulationMetadata
    {
        public PopulationMetadata(Guid incarnation, PopulationState state)
        {
            if (incarnation == Guid.Empty)
            {
                throw new ArgumentException("A population incarnation must be a nonempty generated UUID.", nameof(incarnation));
            }

            if (state != PopulationState.Maintenance && state != PopulationState.Ready)
            {
                throw new ArgumentOutOfRangeException(nameof(state), "Use Maintenance or Ready for the population state.");
            }

            this.Incarnation = incarnation;
            this.State = state;
        }

        public Guid Incarnation { get; }

        public PopulationState State { get; }
    }
}
