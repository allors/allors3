// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Derivations.Rules;
    using Meta;

    public static partial class Rules
    {
        public static Rule[] Create(MetaPopulation m)
        {

            var assembly = typeof(Rules).Assembly;

            var types = assembly.GetTypes()
                .Where(type => type.Namespace != null &&
                               type.GetTypeInfo().IsSubclassOf(typeof(Rule)))
                .ToArray();

            var rules = types.Select(v => Activator.CreateInstance(v, m)).Cast<Rule>().ToArray();

            var duplicates = rules.GroupBy(v => v.Id).Where(g => g.Count() > 1).ToArray();

            if (duplicates.Any())
            {
                throw new Exception("Duplicate rules detected: " + string.Join(", ", duplicates.Select(v => v.Key)));
            }

            return rules;
        }
    }
}
