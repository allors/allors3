// <copyright file="Button.cs" company="Allors bvba">
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

    public class Button : SelectorComponent
    {
        public Button(IPage page, MetaPopulation m, params string[] selectors)
        : base(page, m) =>
            this.Selector = selectors.Length == 1 ? selectors[0] : string.Join(" ", selectors);

        public Button(IPage page, MetaPopulation m, string kind, string value, params string[] scopes)
            : base(page, m) =>
            this.Selector = kind.ToLowerInvariant() switch
            {
                "innertext" => "button[normalize-space()='{value}'{this.ByScopesAnd(scopes)}]",
                _ => "button'{this.ByScopesPredicate(scopes)}"
            };

        public override string Selector { get; }

        public async Task<bool> GetEnabled() => await this.Page.Locator(this.Selector).IsEnabledAsync();

        public async Task Click()
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.Selector);
            await element.ClickAsync();
        }
    }

    public class Button<T> : Button where T : Component
    {
        public Button(T page, MetaPopulation m, params string[] selectors)
            : base(page.Page, m, selectors) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Click()
        {
            await base.Click();
            return this.Page;
        }
    }
}
