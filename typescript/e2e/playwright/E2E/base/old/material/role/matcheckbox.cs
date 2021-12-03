// <copyright file="MatCheckbox.cs" company="Allors bvba">
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

    public class MatCheckbox : SelectorComponent
    {
        public MatCheckbox(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-checkbox{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public override string Selector { get; }

        public ILocator Label => this.Page.Locator($"{this.Selector} label");

        public ILocator Input => this.Page.Locator($"{this.Selector} input");

        public async Task<bool> GetValue()
        {
            await this.Page.WaitForAngular();

            var value = await this.Page.InputValueAsync(this.Selector);
            bool.TryParse(value, out var result);
            return result;
        }

        public async Task SetValue(bool value)
        {
            await this.Page.WaitForAngular();

            var isChecked = await this.Input.IsCheckedAsync();
            if (isChecked)
            {
                if (!value)
                {
                    await this.Label.ClickAsync();
                }
            }
            else if (value)
            {
                await this.Label.ClickAsync();
            }
        }
    }

    public class MatCheckbox<T> : MatCheckbox where T : Component
    {
        public MatCheckbox(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> SetValue(bool value)
        {
            await base.SetValue(value);
            return this.Page;
        }
    }
}
