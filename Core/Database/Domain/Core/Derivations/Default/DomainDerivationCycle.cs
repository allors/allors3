// <copyright file="DomainDerivationCycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain.Derivations.Default
{
    public class DomainDerivationCycle : IDomainDerivationCycle
    {
        public ISession Session { get; internal set; }

        public IChangeSet ChangeSet { get; internal set; }

        public IDomainValidation Validation { get; internal set; }
    }
}
