
// <copyright file="ValidationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain.Derivations
{
    using System;
    using System.Collections.Generic;
    using Workspace.Derivations;
    using Meta;

    public abstract partial class Rule : IRule
    {
        protected Rule(M m, Guid id)
        {
            this.M = m;
            this.Id = id;
        }

        public M M { get; }

        public Guid Id { get; }

        public IEnumerable<IPattern> Patterns { get; protected set; }

        public abstract void Derive(ICycle cycle, IEnumerable<IObject> matches);
    }
}
