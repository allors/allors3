// <copyright file="Cycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain.Derivations.Persistent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Object = Domain.Object;

    public class Iteration : IIteration
    {
        private Properties properties;

        public Iteration(Cycle cycle)
        {
            this.Cycle = cycle;
            this.ChangeSet = new AccumulatedChangeSet();
            this.Graph = new Graph(this.Cycle.Derivation);

            this.Preparations = new Preparation[0];
        }

        ICycle IIteration.Cycle => this.Cycle;

        IPreparation IIteration.Preparation => this.Preparation;

        IAccumulatedChangeSet IIteration.ChangeSet => this.ChangeSet;

        internal Cycle Cycle { get; }

        internal ISet<Object> MarkedBacklog { get; private set; }

        internal Preparation[] Preparations { get; set; }

        internal Preparation Preparation { get; set; }

        internal AccumulatedChangeSet ChangeSet { get; }

        internal Graph Graph { get; }

        public object this[string name]
        {
            get => this.properties?.Get(name);

            set
            {
                this.properties ??= new Properties();
                this.properties.Set(name, value);
            }
        }

        public void Schedule(Object @object) => this.Graph.Schedule(@object);

        public void Mark(Object @object)
        {
            if (@object != null && !this.Graph.IsMarked(@object))
            {
                this.Graph.Mark(@object);
                if (!this.Preparation.Objects.Contains(@object) || this.Preparation.PreDerived.Contains(@object))
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
                // Domain Derivations
                var session = this.Cycle.Derivation.Session;
                AccumulatedChangeSet domainAccumulatedChangeSet = null;
                var domainDerivationById = session.Database.DomainDerivationById;
                if (domainDerivationById.Any())
                {
                    domainAccumulatedChangeSet = new AccumulatedChangeSet();
                    var domainValidation = new DomainValidation(this.Cycle.Derivation.Validation);

                    var changeSet = session.Checkpoint();
                    domainAccumulatedChangeSet.Add(changeSet);

                    while (changeSet.Associations.Any() || changeSet.Roles.Any() || changeSet.Created.Any() || changeSet.Deleted.Any())
                    {
                        // Initialization
                        if (changeSet.Created.Any())
                        {
                            var newObjects = changeSet.Created.Select(v => (Object)v.GetObject());
                            foreach (var newObject in newObjects)
                            {
                                newObject.OnInit();
                            }
                        }

                        var domainCycle = new DomainDerivationCycle { ChangeSet = changeSet, Session = session, Validation = domainValidation };

                        var matchesByDerivationId = new Dictionary<Guid, IEnumerable<IObject>>();
                        foreach (var kvp in domainDerivationById)
                        {
                            var id = kvp.Key;
                            var domainDerivation = kvp.Value;

                            var matches = new HashSet<IObject>();

                            foreach (var pattern in domainDerivation.Patterns)
                            {
                                if (pattern is CreatedPattern createdPattern)
                                {
                                    var created = changeSet.Created.Where(v => v.Class.IsAssignableFrom(createdPattern.Composite)).Select(v => v.GetObject());
                                    matches.UnionWith(created);
                                }

                                if (pattern is RoleChangedPattern changedRolesPattern)
                                {
                                    var changedRoles = changeSet.AssociationsByRoleType
                                        .Where(v => v.Key.Equals(changedRolesPattern.RoleType))
                                        .SelectMany(v => session.Instantiate(v.Value));
                                    matches.UnionWith(changedRoles);
                                }

                                if (pattern is AssociationChangedPattern changedAssociationsPattern)
                                {
                                    var changedAssociations = changeSet.AssociationsByRoleType
                                        .Where(v => v.Key.Equals(changedAssociationsPattern.AssociationType))
                                        .SelectMany(v => session.Instantiate(v.Value));
                                    matches.UnionWith(changedAssociations);
                                }
                            }

                            matchesByDerivationId[id] = matches;
                        }

                        foreach (var kvp in domainDerivationById)
                        {
                            var id = kvp.Key;
                            var domainDerivation = kvp.Value;
                            var matches = matchesByDerivationId[id];
                            domainDerivation.Derive(domainCycle, matches);
                        }

                        changeSet = session.Checkpoint();
                        domainAccumulatedChangeSet.Add(changeSet);

                        this.Cycle.Derivation.DomainDerivationCount++;
                    }
                }

                // Object Derivations
                var config = this.Cycle.Derivation.Config;
                var count = 1;

                if (marked != null)
                {
                    this.Graph.Mark(marked);
                }

                this.Preparation = new Preparation(this, marked, domainAccumulatedChangeSet);
                this.Preparations = this.Preparations.Append(this.Preparation).ToArray();
                this.MarkedBacklog = new HashSet<Object>();
                this.Preparation.Execute();

                while (this.Preparation.Objects.Any() || this.MarkedBacklog.Count > 0)
                {
                    if (config.MaxPreparations != 0 && count++ > config.MaxPreparations)
                    {
                        throw new Exception("Maximum amount of preparations reached");
                    }

                    this.Preparation = new Preparation(this, this.MarkedBacklog);
                    this.Preparations = this.Preparations.Append(this.Preparation).ToArray();
                    this.MarkedBacklog = new HashSet<Object>();
                    this.Preparation.Execute();
                }

                this.Graph.Derive(postDeriveBacklog);

                this.Cycle.Derivation.DerivedObjects.UnionWith(postDeriveBacklog);
            }
            finally
            {
                this.Preparation = null;
            }
        }

        public void AddDependency(Object dependent, params Object[] dependencies) => this.Graph.AddDependency(dependent, dependencies);
    }
}
