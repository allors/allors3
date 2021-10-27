// <copyright file="RulesDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Domain;
    using Object = Domain.Object;

    public class Derivation : IDerivation
    {
        private Properties properties;

        public Derivation(ITransaction transaction, IValidation validation, Engine engine, int maxCycles, bool embedded, bool continueOnError)
        {
            this.Transaction = transaction;
            this.Validation = validation;
            this.Engine = engine;
            this.MaxCycles = maxCycles;
            this.Embedded = embedded;
            this.ContinueOnError = continueOnError;
            this.Id = Guid.NewGuid();
            this.TimeStamp = transaction.Now();
        }

        public Guid Id { get; }

        public DateTime TimeStamp { get; }

        public ITransaction Transaction { get; }

        public IValidation Validation { get; }

        IAccumulatedChangeSet IDerivation.ChangeSet => this.AccumulatedChangeSet;
        public AccumulatedChangeSet AccumulatedChangeSet { get; set; }

        public AccumulatedChangeSet PostDeriveAccumulatedChangeSet { get; set; }

        public Engine Engine { get; }

        public int MaxCycles { get; }

        public bool Embedded { get; }
        public bool ContinueOnError { get; }

        public IValidation Derive()
        {
            var domainCycles = 0;

            this.AccumulatedChangeSet = new AccumulatedChangeSet();
            var changeSet = this.Transaction.Checkpoint();
            this.AccumulatedChangeSet.Add(changeSet);

            if (!this.Embedded)
            {
                this.PostDeriveAccumulatedChangeSet = new AccumulatedChangeSet();
                this.PostDeriveAccumulatedChangeSet.Add(changeSet);
            }

            static bool HasChanges(IChangeSet changeSet) =>
                changeSet.Associations.Any() ||
                changeSet.Roles.Any() ||
                changeSet.Created.Any() ||
                changeSet.Deleted.Any();

            while ((this.ContinueOnError || !this.Validation.HasErrors) && HasChanges(changeSet))
            {
                if (++domainCycles > this.MaxCycles)
                {
                    throw new Exception("Maximum amount of domain derivation cycles detected");
                }

                // Initialization
                if (changeSet.Created.Any())
                {
                    foreach (Object newObject in changeSet.Created)
                    {
                        newObject.OnInit();
                    }
                }

                var domainCycle = new Cycle
                {
                    ChangeSet = changeSet,
                    Transaction = this.Transaction,
                    Validation = this.Validation
                };


                var matchesByRule = new Dictionary<IRule, ISet<IObject>>();

                foreach (var kvp in changeSet.AssociationsByRoleType)
                {
                    var roleType = kvp.Key;
                    foreach (var association in kvp.Value)
                    {
                        var strategy = association.Strategy;
                        var @class = strategy.Class;

                        if (this.Engine.PatternsByRoleTypeByClass.TryGetValue(@class, out var patternsByRoleType) && patternsByRoleType.TryGetValue(roleType, out var patterns))
                        {
                            foreach (var pattern in patterns)
                            {
                                var rule = this.Engine.RuleByPattern[pattern];
                                if (!matchesByRule.TryGetValue(rule, out var matches))
                                {
                                    matches = new HashSet<IObject>();
                                    matchesByRule.Add(rule, matches);
                                }

                                IEnumerable<IObject> source = new IObject[] { association };

                                if (pattern.Tree != null)
                                {
                                    source = source.SelectMany(v => pattern.Tree.SelectMany(w => w.Resolve(v)));
                                }

                                if (pattern.OfType != null)
                                {
                                    source = source.Where(v => pattern.OfType.IsAssignableFrom(v.Strategy.Class));
                                }

                                matches.UnionWith(source);
                            }
                        }
                    }
                }

                foreach (var kvp in changeSet.RolesByAssociationType)
                {
                    var associationType = kvp.Key;
                    foreach (var role in this.Transaction.Instantiate(kvp.Value))
                    {
                        var strategy = role.Strategy;
                        var @class = strategy.Class;

                        if (this.Engine.PatternsByAssociationTypeByClass.TryGetValue(@class, out var patternsByAssociationType) && patternsByAssociationType.TryGetValue(associationType, out var patterns))
                        {
                            foreach (var pattern in patterns)
                            {
                                var rule = this.Engine.RuleByPattern[pattern];
                                if (!matchesByRule.TryGetValue(rule, out var matches))
                                {
                                    matches = new HashSet<IObject>();
                                    matchesByRule.Add(rule, matches);
                                }

                                IEnumerable<IObject> source = new IObject[] { role };

                                if (pattern.Tree != null)
                                {
                                    source = source.SelectMany(v => pattern.Tree.SelectMany(w => w.Resolve(v)));
                                }

                                if (pattern.OfType != null)
                                {
                                    source = source.Where(v => pattern.OfType.IsAssignableFrom(v.Strategy.Class));
                                }

                                matches.UnionWith(source);
                            }
                        }
                    }
                }

                // TODO: Prefetching

                foreach (var kvp in matchesByRule)
                {
                    var domainDerivation = kvp.Key;
                    var matches = kvp.Value;
                    domainDerivation.Derive(domainCycle, matches);
                }

                changeSet = this.Transaction.Checkpoint();
                this.AccumulatedChangeSet.Add(changeSet);

                if (!this.Embedded)
                {
                    if (HasChanges(changeSet))
                    {
                        this.PostDeriveAccumulatedChangeSet.Add(changeSet);
                    }
                    else
                    {
                        var created = this.PostDeriveAccumulatedChangeSet.Created;
                        foreach (Object @object in created)
                        {
                            @object.OnPostDerive(x => x.WithDerivation(this));
                        }

                        foreach (Object @object in this.PostDeriveAccumulatedChangeSet.Associations)
                        {
                            if (!created.Contains(@object))
                            {
                                @object.OnPostDerive(x => x.WithDerivation(this));
                            }
                        }

                        this.PostDeriveAccumulatedChangeSet = new AccumulatedChangeSet();

                        changeSet = this.Transaction.Checkpoint();
                    }
                }
            }

            // Required check
            foreach (var grouping in this.AccumulatedChangeSet.Created
                .Where(v => !this.AccumulatedChangeSet.Deleted.Contains(v.Strategy))
                .GroupBy(v => v.Strategy.Class))
            {
                var @class = grouping.Key;
                foreach (var @object in grouping)
                {
                    foreach (var roleType in @class.RequiredRoleTypes)
                    {
                        this.Validation.AssertExists(@object, roleType);
                    }
                }
            }

            foreach (var grouping in this.AccumulatedChangeSet.Associations
                .Except(this.AccumulatedChangeSet.Created)
                .Where(v => !this.AccumulatedChangeSet.Deleted.Contains(v.Strategy))
                .GroupBy(v => v.Strategy.Class))
            {
                var @class = grouping.Key;

                // TODO: Prefetch
                foreach (var @object in grouping)
                {
                    foreach (var roleType in @class.RequiredRoleTypes)
                    {
                        this.Validation.AssertExists(@object, roleType);
                    }
                }
            }

            return this.Validation;
        }

        public object this[string name]
        {
            get => this.properties?.Get(name);

            set
            {
                this.properties ??= new Properties();
                this.properties.Set(name, value);
            }
        }
    }
}
