// <copyright file="Input.cs" company="Allors bvba">
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

    public class Input : SelectorComponent
    {
        public Input(IPage page, MetaPopulation m, params string[] selectors)
            : base(page, m) =>
            this.Selector = selectors.Length == 1 ? selectors[0] : string.Join(" ", selectors);

        public Input(IPage page, MetaPopulation m, string kind, string value, params string[] scopes)
            : base(page, m) =>
            this.Selector = kind.ToLowerInvariant() switch
            {
                "id" => $"input[@id='{value}'{this.ByScopesAnd(scopes)}]",
                "name" => $"input[@name='{value}'{this.ByScopesAnd(scopes)}]",
                "formcontrolname" => $"input[@formcontrolname='{value}'{this.ByScopesAnd(scopes)}]",
                _ => $"input{this.ByScopesPredicate(scopes)}"
            };

        public override string Selector { get; }

        public async Task<string> GetValue()
        {
            await this.Page.WaitForAngular();

            var element = this.Page.Locator(this.Selector);
            return await element.InputValueAsync();
        }

        public async Task SetValue(string value)
        {
            await this.Page.WaitForAngular();

            var element = this.Page.Locator(this.Selector);
            await element.FillAsync(value);
        }
    }

    public class Input<T> : Input where T : Component
    {
        public Input(T page, MetaPopulation m, params string[] selectors)
            : base(page.Page, m, selectors) =>
            this.Page = page;

        public Input(T page, MetaPopulation m, string kind, string value, params string[] scopes)
            : base(page.Page, m, kind, value, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> SetValue(string value)
        {
            await base.SetValue(value);
            return this.Page;
        }
    }
}
