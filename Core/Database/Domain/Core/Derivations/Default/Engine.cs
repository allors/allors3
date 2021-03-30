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

        public IDictionary<IClass, Rule[]> RulesByClass { get; }

        public Engine(MetaPopulation m, Rule[] rules)
        {
            this.ClassesByRule = new Dictionary<Rule, ISet<IClass>>();

            foreach (var rule in rules)
            {
                var ruleClasses = new HashSet<IClass>();
                foreach (var pattern in rule.Patterns)
                {
                    var patternClasses = pattern switch
                    {
                        RolePattern { ObjectType: null } rolePattern => rolePattern.RoleType.AssociationType.ObjectType.DatabaseClasses,
                        RolePattern { ObjectType: { } } rolePattern => rolePattern.ObjectType.DatabaseClasses,
                        AssociationPattern associationPattern => ((Composite)associationPattern.AssociationType.ObjectType).DatabaseClasses,
                        _ => Array.Empty<Class>()
                    };

                    ruleClasses.UnionWith(patternClasses);
                }

                this.ClassesByRule.Add(rule, ruleClasses);
            }

            this.RulesByClass = m.Classes.Cast<IClass>().ToDictionary(v => v, v => rules.Where(w => this.ClassesByRule[w].Contains(v)).ToArray());
        }
    }
}
