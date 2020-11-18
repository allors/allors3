
// <copyright file="ValidationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using Database.Derivations;
    using Meta;

    public abstract partial class DomainDerivation : IDomainDerivation
    {
        protected DomainDerivation(M m, Guid id)
        {
            this.M = m;
            this.Id = id;
        }

        public M M { get; set; }

        public Guid Id { get; protected set; }

        public Pattern[] Patterns { get; protected set; }

        public abstract void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches);
    }
}
