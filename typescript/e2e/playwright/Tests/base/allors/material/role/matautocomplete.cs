// <copyright file="MatAutoComplete.cs" company="Allors bvba">
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

    public class MatAutocomplete : SelectorComponent
    {
        public MatAutocomplete(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes) : base(page, m) => this.Selector = $"a-mat-autocomplete{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public ILocator Input => this.Page.Locator($"{this.Selector} input");

        public override string Selector { get; }

        public async Task Select(string value, string selection = null)
        {
            await this.Page.WaitForAngular();

            await this.Input.FillAsync(value);

            await this.Page.WaitForAngular();

            value = this.CssEscape(value);
            var optionSelector = $"{this.Selector} mat-option[data-allors-option-display='{selection ?? value}'] span";
            var option = this.Page.Locator(optionSelector);
            await option.ClickAsync();
        }
    }

    public class MatAutocomplete<T> : MatAutocomplete where T : Component
    {
        public MatAutocomplete(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Select(string value, string selection = null)
        {
            await base.Select(value, selection);
            return this.Page;
        }
    }
}
