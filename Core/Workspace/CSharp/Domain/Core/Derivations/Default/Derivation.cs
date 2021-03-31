// <copyright file="Cycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Meta;
    using Domain;

    public class Derivation : IDerivation
    {
        private readonly ISession session;

        private readonly Engine engine;

        private readonly int maxDomainDerivationCycles;

        public Derivation(ISession session, Engine engine, int maxDomainDerivationCycles)
        {
            this.session = session;
            this.engine = engine;
            this.maxDomainDerivationCycles = maxDomainDerivationCycles;

            this.Validation = new Validation();
        }

        public IValidation Validation { get; }

        public IValidation Execute()
        {
            var cycles = 0;

            var changeSet = this.session.Checkpoint();

            while (changeSet.RolesByAssociationType?.Count > 0 || changeSet.AssociationsByRoleType?.Count > 0 || changeSet.Created?.Count > 0 || changeSet.Instantiated?.Count > 0)
            {
                if (++cycles > this.maxDomainDerivationCycles)
                {
                    throw new Exception("Maximum amount of domain derivation cycles detected");
                }

                var cycle = new Cycle
                {
                    ChangeSet = changeSet,
                    Session = this.session,
                    Validation = this.Validation
                };

                var matchesByRule = new Dictionary<IRule, ISet<IObject>>();

                if (changeSet.Instantiated != null)
                {
                    foreach (var instantiated in changeSet?.Instantiated)
                    {
                        var @class = (Class)instantiated.Class;
                        if (this.engine.RulesByClass.TryGetValue(@class, out var rules))
                        {
                            foreach (var rule in rules)
                            {
                                if (!matchesByRule.TryGetValue(rule, out var matches))
                                {
                                    matches = new HashSet<IObject>();
                                    matchesByRule.Add(rule, matches);
                                }

                                matches.Add(instantiated.Object);
                            }
                        }
                    }
                }

                foreach (var kvp in changeSet.AssociationsByRoleType)
                {
                    var roleType = kvp.Key;
                    var associations = kvp.Value;

                    foreach (var association in associations)
                    {
                        var @class = association.Class;

                        if (this.engine.PatternsByRoleTypeByClass.TryGetValue(@class, out var patternsByRoleType))
                        {
                            if (patternsByRoleType.TryGetValue(roleType, out var patterns))
                            {
                                foreach (var pattern in patterns)
                                {
                                    var rule = this.engine.RuleByPattern[pattern];
                                    if (!matchesByRule.TryGetValue(rule, out var matches))
                                    {
                                        matches = new HashSet<IObject>();
                                        matchesByRule.Add(rule, matches);
                                    }

                                    IEnumerable<IObject> source = new IObject[] { association.Object };

                                    if (pattern.Steps?.Length > 0)
                                    {
                                        var step = new Step(pattern.Steps);
                                        source = source.SelectMany(v => step.Get(v));
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
                    var roles = kvp.Value;

                    foreach (var role in roles)
                    {
                        var @class = role.Class;

                        if (this.engine.PatternsByAssociationTypeByClass.TryGetValue(@class, out var patternsByAssociationType))
                        {
                            if (patternsByAssociationType.TryGetValue(associationType, out var patterns))
                            {
                                foreach (var pattern in patterns)
                                {
                                    var rule = this.engine.RuleByPattern[pattern];
                                    if (!matchesByRule.TryGetValue(rule, out var matches))
                                    {
                                        matches = new HashSet<IObject>();
                                        matchesByRule.Add(rule, matches);
                                    }

                                    IEnumerable<IObject> source = new IObject[] { role.Object };

                                    if (pattern.Steps?.Length > 0)
                                    {
                                        var step = new Step(pattern.Steps);
                                        source = source.SelectMany(v => step.Get(v));
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

                foreach (var kvp in matchesByRule)
                {
                    var domainDerivation = kvp.Key;
                    var matches = kvp.Value;
                    domainDerivation.Derive(cycle, matches);
                }

                changeSet = this.session.Checkpoint();
            }

            return this.Validation;
        }
    }
}
