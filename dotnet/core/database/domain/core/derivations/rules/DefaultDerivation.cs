// <copyright file="DomainDerivationCycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Object = Domain.Object;

    public class DefaultDerivation : IDerivation
    {
        public DefaultDerivation(ITransaction transaction, IValidation validation, Engine engine, int maxCycles)
        {
            this.Transaction = transaction;
            this.Validation = validation;
            this.Engine = engine;
            this.MaxCycles = maxCycles;
        }

        public ITransaction Transaction { get; }

        public IValidation Validation { get; }

        IAccumulatedChangeSet IDerivation.ChangeSet => this.ChangeSet;
        public AccumulatedChangeSet ChangeSet { get; set; }
        
        public Engine Engine { get; }

        public int MaxCycles { get; }

        public IValidation Derive()
        {
            var domainCycles = 0;

            this.ChangeSet = new AccumulatedChangeSet();

            var changeSet = this.Transaction.Checkpoint();
            this.ChangeSet.Add(changeSet);

            while (changeSet.Associations.Any() || changeSet.Roles.Any() || changeSet.Created.Any() ||
                   changeSet.Deleted.Any())
            {
                if (++domainCycles > this.MaxCycles)
                {
                    throw new Exception("Maximum amount of domain derivation cycles detected");
                }

                // Initialization
                if (changeSet.Created.Any())
                {
                    var newObjects = changeSet.Created.Select(v => (Object)v.GetObject());
                    foreach (var newObject in newObjects)
                    {
                        _ = newObject.OnInit();
                    }
                }

                var domainCycle = new DomainDerivationCycle
                {
                    ChangeSet = changeSet,
                    Transaction = this.Transaction,
                    Validation = this.Validation
                };


                var matchesByRule = new Dictionary<IRule, ISet<IObject>>();

                foreach (var kvp in changeSet.AssociationsByRoleType)
                {
                    var roleType = kvp.Key;
                    var associations = this.Transaction.Instantiate(kvp.Value);

                    foreach (var association in associations)
                    {
                        var strategy = association.Strategy;
                        var @class = strategy.Class;

                        if (this.Engine.PatternsByRoleTypeByClass.TryGetValue(@class, out var patternsByRoleType))
                        {
                            if (patternsByRoleType.TryGetValue(roleType, out var patterns))
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
                }

                foreach (var kvp in changeSet.RolesByAssociationType)
                {
                    var associationType = kvp.Key;
                    var roles = this.Transaction.Instantiate(kvp.Value);

                    foreach (var role in roles)
                    {
                        var strategy = role.Strategy;
                        var @class = strategy.Class;

                        if (this.Engine.PatternsByAssociationTypeByClass.TryGetValue(@class, out var patternsByAssociationType))
                        {
                            if (patternsByAssociationType.TryGetValue(associationType, out var patterns))
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
                }

                // TODO: Prefetching

                foreach (var kvp in matchesByRule)
                {
                    var domainDerivation = kvp.Key;
                    var matches = kvp.Value;
                    domainDerivation.Derive(domainCycle, matches);
                }

                changeSet = this.Transaction.Checkpoint();
                this.ChangeSet.Add(changeSet);
            }

            return this.Validation;
        }
    }
}
