// <copyright file="MatTextarea.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class MatLocalisedMarkdown : SelectorComponent
    {
        public MatLocalisedMarkdown(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-localised-markdown{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public override string Selector { get; }

        public async Task<string> GetValue()
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.Selector);
            return await element.GetAttributeAsync("value");
        }

        public async Task SetValue(string value)
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.Selector);

            var expression =
    $@"const element = arguments[0];
element.easyMDE.value('{value}');";

            await this.Page.EvaluateAsync<bool?>(expression, element);
        }
    }

    public class MatLocalisedMarkdown<T> : MatTextarea where T : Component
    {
        public MatLocalisedMarkdown(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> SetValue(string value)
        {
            await this.SetValue(value);
            return this.Page;
        }
    }
}
