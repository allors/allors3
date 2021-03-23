
// <copyright file="ValidationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Derivations
{
    using System;
    using System.Collections.Generic;
    using Meta;

    public abstract partial class Derivation : IDerivation
    {
        protected Derivation(M m, Guid id)
        {
            this.M = m;
            this.Id = id;
        }

        private M M { get; set; }

        public Guid Id { get; protected set; }

        public Pattern[] Patterns { get; protected set; }

        public abstract void Derive(IDerivationCycle cycle, IEnumerable<IObject> matches);
    }
}
