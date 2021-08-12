// <copyright file="DerivationNode.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
    internal class LegacyNode : IEquatable<LegacyNode>
    {
        private readonly Domain.Object derivable;
        private LegacyNode currentRoot;

        private bool isVisited;

        internal LegacyNode(Domain.Object derivable) => this.derivable = derivable;

        internal bool IsMarked { get; set; }

        internal bool IsScheduled { get; set; }

        internal LegacyNode[] Dependencies { get; set; }

        public bool Equals(LegacyNode other) => other != null && this.derivable.Equals(other.derivable);

        public override bool Equals(object obj) => this.Equals((LegacyNode)obj);

        public override int GetHashCode() => this.derivable.GetHashCode();

        internal void TopologicalDerive(LegacyGraph graph, IList<Domain.Object> postDeriveBacklog) => this.TopologicalDerive(graph, postDeriveBacklog, this);

        private void TopologicalDerive(LegacyGraph graph, IList<Domain.Object> postDeriveBacklog, LegacyNode root)
        {
            if (this.isVisited)
            {
                if (root.Equals(this.currentRoot))
                {
                    throw new Exception("This derivation has a cycle. (" + this.currentRoot + " -> " + this + ")");
                }

                return;
            }

            this.isVisited = true;
            this.currentRoot = root;

            if (this.Dependencies != null)
            {
                foreach (var dep in this.Dependencies)
                {
                    dep.TopologicalDerive(graph, postDeriveBacklog, root);
                }
            }

            if (!this.derivable.Strategy.IsDeleted && graph.IsScheduled(this.derivable))
            {
                // TODO: Remove OnDerive
                this.derivable.OnDerive(x => x.WithDerivation(graph.Derivation));
                postDeriveBacklog.Add(this.derivable);
            }

            this.currentRoot = null;
        }
    }
}
