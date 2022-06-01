
// <copyright file="Iteration.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Domain.Derivations.Legacy;
    using Object = Domain.Object;

    public class LegacyIteration : ILegacyIteration
    {
        private LegacyProperties legacyProperties;

        public LegacyIteration(LegacyCycle legacyCycle)
        {
            this.LegacyCycle = legacyCycle;
            this.ChangeSet = new AccumulatedChangeSet();
            this.Graph = new LegacyGraph(this.LegacyCycle.Derivation);
        }

        ILegacyCycle ILegacyIteration.LegacyCycle => this.LegacyCycle;

        ILegacyPreparation ILegacyIteration.LegacyPreparation => this.LegacyPreparation;

        IAccumulatedChangeSet ILegacyIteration.ChangeSet => this.ChangeSet;

        internal LegacyCycle LegacyCycle { get; }

        internal ISet<Object> MarkedBacklog { get; private set; }

        internal LegacyPreparation LegacyPreparation { get; set; }

        internal AccumulatedChangeSet ChangeSet { get; }

        internal LegacyGraph Graph { get; }

        public object this[string name]
        {
            get => this.legacyProperties?.Get(name);

            set
            {
                this.legacyProperties ??= new LegacyProperties();
                this.legacyProperties.Set(name, value);
            }
        }

        public void Schedule(Object @object) => this.Graph.Schedule(@object);

        public void Mark(Object @object)
        {
            if (@object != null && !this.Graph.IsMarked(@object))
            {
                this.Graph.Mark(@object);
                if (!this.LegacyPreparation.Objects.Contains(@object) || this.LegacyPreparation.PreDerived.Contains(@object))
                {
                    this.MarkedBacklog.Add(@object);
                }
            }
        }

        public void Mark(params Object[] objects)
        {
            foreach (var @object in objects)
            {
                this.Mark(@object);
            }
        }

        public bool IsMarked(Object @object) => this.Graph.IsMarked(@object);

        public void Execute(List<Object> postDeriveBacklog, Object[] marked = null)
        {
            try
            {
                var domainDerive = new Derivation(this.LegacyCycle.Derivation.Transaction, this.LegacyCycle.Derivation.Validation, this.LegacyCycle.Derivation.Engine, this.LegacyCycle.Derivation.MaxCycles, true, this.LegacyCycle.Derivation.ContinueOnError);
                domainDerive.Derive();

                // Object Derivations
                var config = this.LegacyCycle.Derivation.LegacyDerivationConfig;
                var count = 1;

                if (marked != null)
                {
                    this.Graph.Mark(marked);
                }

                this.LegacyPreparation = new LegacyPreparation(this, marked, domainDerive.AccumulatedChangeSet);
                this.MarkedBacklog = new HashSet<Object>();
                this.LegacyPreparation.Execute();

                while (this.LegacyPreparation.Objects.Any() || this.MarkedBacklog.Count > 0)
                {
                    if (config.MaxPreparations != 0 && count++ > config.MaxPreparations)
                    {
                        throw new Exception("Maximum amount of preparations reached");
                    }

                    this.LegacyPreparation = new LegacyPreparation(this, this.MarkedBacklog);
                    this.MarkedBacklog = new HashSet<Object>();
                    this.LegacyPreparation.Execute();
                }

                this.Graph.Derive(postDeriveBacklog);

                this.LegacyCycle.Derivation.DerivedObjects.UnionWith(postDeriveBacklog);
            }
            finally
            {
                this.LegacyPreparation = null;
            }
        }

        public void AddDependency(Object dependent, params Object[] dependencies) => this.Graph.AddDependency(dependent, dependencies);
    }
}
