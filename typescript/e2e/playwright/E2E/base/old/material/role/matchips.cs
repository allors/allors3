// <copyright file="MatChips.cs" company="Allors bvba">
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

    public class MatChips : SelectorComponent
    {
        public MatChips(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page, m) =>
                this.Selector = $"a-mat-chips{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public override string Selector { get; }

        public ILocator Input => this.Page.Locator($"{this.Selector} input");

        public async Task Add(string value, string selection = null)
        {
            await this.Page.WaitForAngular();

            await this.Input.FillAsync(value);

            await this.Page.WaitForAngular();

            value = this.CssEscape(value);
            var optionSelector = $"mat-option[data-allors-option-display='{selection ?? value}'] span";
            var option = this.Page.Locator(optionSelector);
            await option.ClickAsync();
        }

        public async Task Remove(string value)
        {
            await this.Page.WaitForAngular();

            var listItem = this.Page.Locator($"{this.Selector} mat-chip[data-allors-chip-display='{value}'] mat-icon");
            await listItem.ClickAsync();
        }
    }

    public class MatChips<T> : MatChips where T : Component
    {
        public MatChips(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Add(string value, string selection = null)
        {
            await base.Add(value, selection);
            return this.Page;
        }

        public new async Task<T> Remove(string value)
        {
            await base.Remove(value);
            return this.Page;
        }
    }
}
