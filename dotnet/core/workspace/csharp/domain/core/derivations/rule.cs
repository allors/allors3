
// <copyright file="ValidationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain.Derivations
{
    using System.Collections.Generic;
    using Workspace.Derivations;
    using Meta;

    public abstract partial class Rule : IRule
    {
        protected Rule(M m) => this.M = m;

        public M M { get; }

        public IComposite ObjectType { get; protected set; }

        public IRoleType RoleType { get; protected set; }

        public IEnumerable<IDependency> Dependencies { get; protected set; }

        public abstract object Derive(IObject match);
    }
}
