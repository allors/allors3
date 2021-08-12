
// <copyright file="ValidationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Rules
{
    using System;
    using System.Collections.Generic;
    using Database.Derivations;
    using Meta;

    public abstract class Rule : IRule
    {
        protected Rule(MetaPopulation m, Guid id)
        {
            this.M = m;
            this.Id = id;
        }

        public MetaPopulation M { get; }

        public Guid Id { get; }

        public IEnumerable<IPattern> Patterns { get; protected set; }

        public abstract void Derive(ICycle cycle, IEnumerable<IObject> matches);
    }
}
