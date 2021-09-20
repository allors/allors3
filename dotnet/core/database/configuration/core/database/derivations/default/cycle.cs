// <copyright file="DomainDerivationCycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using Database.Derivations;

    public class Cycle : ICycle
    {
        public ITransaction Transaction { get; internal set; }

        public IChangeSet ChangeSet { get; internal set; }

        public IValidation Validation { get; internal set; }
    }
}
