// <copyright file="IPattern.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Domain.Derivations.Rules
{
    using System.Collections.Generic;
    using Database.Data;
    using Database.Derivations;
    using Meta;

    public abstract class Pattern : IPattern
    {
        IEnumerable<Node> IPattern.Tree => this.Tree;
        public IEnumerable<Node> Tree { get; set; }

        public IComposite OfType { get; set; }

        public abstract IComposite ObjectType { get; }
    }
}
