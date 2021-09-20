// <copyright file="ICycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Legacy
{
    using Database.Derivations;

    public interface ILegacyCycle
    {
        IAccumulatedChangeSet ChangeSet { get; }

        object this[string name] { get; set; }

        ILegacyDerivation Derivation { get; }

        ILegacyIteration LegacyIteration { get; }
    }
}
