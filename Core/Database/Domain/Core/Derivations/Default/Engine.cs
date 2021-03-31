// <copyright file="DerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;

    public class Engine
    {
        public IDictionary<Rule, ISet<IClass>> ClassesByRule { get; }

        public IDictionary<IClass, IDictionary<IRoleType, ISet<RolePattern>>> PatternsByRoleTypeByClass { get; }

        public IDictionary<IClass, IDictionary<IAssociationType, ISet<AssociationPattern>>> PatternsByAssociationTypeByClass { get; }

        public IDictionary<Pattern, IRule> RuleByPattern { get; }

        public Engine(Rule[] rules)
        {
            this.ClassesByRule = new Dictionary<Rule, ISet<IClass>>();
            this.PatternsByRoleTypeByClass = new Dictionary<IClass, IDictionary<IRoleType, ISet<RolePattern>>>();
            this.PatternsByAssociationTypeByClass = new Dictionary<IClass, IDictionary<IAssociationType, ISet<AssociationPattern>>>();
            this.RuleByPattern = new Dictionary<Pattern, IRule>();

            foreach (var rule in rules)
            {
                var ruleClasses = new HashSet<IClass>();
                foreach (var pattern in rule.Patterns)
                {
                    this.RuleByPattern.Add(pattern, rule);

                    var patternClasses = pattern switch
                    {
                        RolePattern { ObjectType: null } rolePattern => rolePattern.RoleType.AssociationType.ObjectType.DatabaseClasses.ToArray(),
                        RolePattern { ObjectType: { } } rolePattern => rolePattern.ObjectType.DatabaseClasses.ToArray(),
                        AssociationPattern associationPattern => associationPattern.AssociationType.RoleType.ObjectType.IsComposite ? ((Composite)associationPattern.AssociationType.RoleType.ObjectType).DatabaseClasses.ToArray() : Array.Empty<IClass>(),
                        _ => Array.Empty<IClass>()
                    };

                    ruleClasses.UnionWith(patternClasses);

                    switch (pattern)
                    {
                        case RolePattern rolePattern:
                            foreach (var @class in patternClasses)
                            {
                                if (!this.PatternsByRoleTypeByClass.TryGetValue(@class, out var patternsByRoleType))
                                {
                                    patternsByRoleType = new Dictionary<IRoleType, ISet<RolePattern>>();
                                    this.PatternsByRoleTypeByClass.Add(@class, patternsByRoleType);
                                }

                                var roleType = rolePattern.RoleType;

                                if (!patternsByRoleType.TryGetValue(roleType, out var patterns))
                                {
                                    patterns = new HashSet<RolePattern>();
                                    patternsByRoleType.Add(roleType, patterns);
                                }

                                patterns.Add(rolePattern);
                            }

                            break;
                        case AssociationPattern associationPattern:
                            foreach (var @class in patternClasses)
                            {
                                if (!this.PatternsByAssociationTypeByClass.TryGetValue(@class, out var patternsByAssociationType))
                                {
                                    patternsByAssociationType = new Dictionary<IAssociationType, ISet<AssociationPattern>>();
                                    this.PatternsByAssociationTypeByClass.Add(@class, patternsByAssociationType);
                                }

                                var associationType = associationPattern.AssociationType;

                                if (!patternsByAssociationType.TryGetValue(associationType, out var patterns))
                                {
                                    patterns = new HashSet<AssociationPattern>();
                                    patternsByAssociationType.Add(associationType, patterns);
                                }

                                patterns.Add(associationPattern);
                            }

                            break;
                    }
                }

                this.ClassesByRule.Add(rule, ruleClasses);
            }
        }
    }
}
