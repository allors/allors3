// <copyright file="Engine.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;

    public class Engine
    {
        public IDictionary<IRule, ISet<IClass>> ClassesByRule { get; }

        public IDictionary<IClass, IDictionary<IRoleType, ISet<IRolePattern>>> PatternsByRoleTypeByClass { get; }

        public IDictionary<IClass, IDictionary<IAssociationType, ISet<IAssociationPattern>>> PatternsByAssociationTypeByClass { get; }

        public IDictionary<IPattern, IRule> RuleByPattern { get; }

        public Engine(IRule[] rules)
        {
            this.ClassesByRule = new Dictionary<IRule, ISet<IClass>>();
            this.PatternsByRoleTypeByClass = new Dictionary<IClass, IDictionary<IRoleType, ISet<IRolePattern>>>();
            this.PatternsByAssociationTypeByClass = new Dictionary<IClass, IDictionary<IAssociationType, ISet<IAssociationPattern>>>();
            this.RuleByPattern = new Dictionary<IPattern, IRule>();

            foreach (var rule in rules)
            {
                var ruleClasses = new HashSet<IClass>();
                foreach (var pattern in rule.Patterns)
                {
                    this.RuleByPattern.Add(pattern, rule);

                    var patternClasses = pattern switch
                    {
                        IRolePattern { ObjectType: null } rolePattern => rolePattern.RoleType.AssociationType.ObjectType.DatabaseClasses.ToArray(),
                        IRolePattern { ObjectType: { } } rolePattern => rolePattern.ObjectType.DatabaseClasses.ToArray(),

                        IAssociationPattern { ObjectType: null } associationPattern => associationPattern.AssociationType.RoleType.ObjectType.IsComposite ? ((Composite)associationPattern.AssociationType.RoleType.ObjectType).DatabaseClasses.ToArray() : Array.Empty<IClass>(),
                        IAssociationPattern { ObjectType: { } } associationPattern => associationPattern.ObjectType.DatabaseClasses.ToArray(),

                        _ => Array.Empty<IClass>()
                    };

                    ruleClasses.UnionWith(patternClasses);

                    switch (pattern)
                    {
                        case IRolePattern rolePattern:
                            foreach (var @class in patternClasses)
                            {
                                if (!this.PatternsByRoleTypeByClass.TryGetValue(@class, out var patternsByRoleType))
                                {
                                    patternsByRoleType = new Dictionary<IRoleType, ISet<IRolePattern>>();
                                    this.PatternsByRoleTypeByClass.Add(@class, patternsByRoleType);
                                }

                                var roleType = rolePattern.RoleType;

                                if (!patternsByRoleType.TryGetValue(roleType, out var patterns))
                                {
                                    patterns = new HashSet<IRolePattern>();
                                    patternsByRoleType.Add(roleType, patterns);
                                }

                                patterns.Add(rolePattern);
                            }

                            break;
                        case IAssociationPattern associationPattern:
                            foreach (var @class in patternClasses)
                            {
                                if (!this.PatternsByAssociationTypeByClass.TryGetValue(@class, out var patternsByAssociationType))
                                {
                                    patternsByAssociationType = new Dictionary<IAssociationType, ISet<IAssociationPattern>>();
                                    this.PatternsByAssociationTypeByClass.Add(@class, patternsByAssociationType);
                                }

                                var associationType = associationPattern.AssociationType;

                                if (!patternsByAssociationType.TryGetValue(associationType, out var patterns))
                                {
                                    patterns = new HashSet<IAssociationPattern>();
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
