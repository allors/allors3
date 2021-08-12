// <copyright file="ObjectOnPreDerive.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Derivations;
    using Derivations.Legacy;

    public abstract partial class ObjectOnPreDerive
    {
        public ILegacyIteration LegacyIteration { get; set; }

        public ObjectOnPreDerive WithIteration(ILegacyIteration legacyIteration)
        {
            this.LegacyIteration = legacyIteration;
            return this;
        }

        public void Deconstruct(out ILegacyIteration legacyIteration, out IChangeSet changeSet, out ISet<Object> derivedObjects)
        {
            changeSet = this.LegacyIteration.ChangeSet;
            legacyIteration = this.LegacyIteration;
            derivedObjects = this.LegacyIteration.LegacyCycle.Derivation.DerivedObjects;
        }
    }
}
