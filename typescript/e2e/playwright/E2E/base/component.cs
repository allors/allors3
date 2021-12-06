// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Linq;
    using Allors.Database.Meta;
    using Microsoft.Playwright;

    public abstract class Component
    {
        protected Component(IPage page, MetaPopulation m)
        {
            this.Page = page;
            this.M = m;
        }

        public IPage Page { get; }

        public MetaPopulation M { get; set; }

        public static string[] ByScopesExpressions(params string[] scopes) => scopes.Select((v, i) => $"ancestor::*[@data-test-scope][{i + 1}]/@data-test-scope='{v}'").ToArray();

        public string ByScopesPredicate(string[] scopes) => scopes.Length > 0 ? $"[{string.Join(" and ", ByScopesExpressions(scopes))}]" : string.Empty;

        public string ByScopesAnd(string[] scopes) => string.Concat(ByScopesExpressions(scopes).Select(v => $" and {v}"));
    }
}
