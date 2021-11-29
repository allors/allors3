// <copyright file="MatStatic.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;

    public class MatStatic : SelectorComponent
    {
        public MatStatic(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-static{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public MatStatic(IPage page, MetaPopulation m, string selector)
            : base(page, m) =>
            this.Selector = selector;

        public override string Selector { get; }

        public async Task<string> GetValue()
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.Selector);
            return await element.TextContentAsync();
        }
    }

    public class MatStatic<T> : MatStatic where T : Component
    {
        public MatStatic(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }
    }
}
