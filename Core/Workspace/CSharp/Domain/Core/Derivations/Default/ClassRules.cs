// <copyright file="DerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Derivations.Default
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class ClassRules
    {
        public ClassRules(Class @class, Rule[] rules, IDictionary<Rule, ISet<Class>> classesByRule) => this.Rules = rules.Where(v => classesByRule[v].Contains(@class)).ToArray();

        public Rule[] Rules { get; }
    }
}
