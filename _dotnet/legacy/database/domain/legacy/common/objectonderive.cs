// <copyright file="ObjectOnDerive.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Derivations.Legacy;

    public abstract partial class ObjectOnDerive
    {
        public ILegacyDerivation Derivation { get; set; }

        public ObjectOnDerive WithDerivation(ILegacyDerivation derivation)
        {
            this.Derivation = derivation;
            return this;
        }
    }
}
