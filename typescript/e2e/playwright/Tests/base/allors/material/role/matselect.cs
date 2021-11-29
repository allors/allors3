// <copyright file="MatSelect.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class MatSelect : SelectorComponent
    {
        public MatSelect(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes) : base(page, m)
        {
            this.Selector = "a-mat-select{this.ByScopesPredicate(scopes)}";
            this.ArrowSelector = $"{this.Selector} mat-select[@data-allors-roletype='{roleType.RelationType.Tag}'] [contains(@class,'mat-select-arrow')]";
            this.ValueTextSelector = $"{this.Selector} mat-select[@data-allors-roletype='{roleType.RelationType.Tag}'] [contains(@class,'mat-select-value-text')]";
        }

        public override string Selector { get; }

        private string ArrowSelector { get; }

        private string ValueTextSelector { get; }

        public async Task<string> GetValue()
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.ValueTextSelector);
            return await element.TextContentAsync();
        }

        public async Task SetValue(string value)
        {
            await this.Page.WaitForAngular();

            var arrow = this.Page.Locator(this.ArrowSelector);
            await arrow.ClickAsync();

            await this.Page.WaitForAngular();

            var optionSelector = $"mat-option[data-allors-option-display='{value}'] span";
            var option = this.Page.Locator(optionSelector);
            await option.ClickAsync();
        }

        public async Task Select(IObject @object)
        {
            await this.Page.WaitForAngular();

            var arrow = this.Page.Locator(this.ArrowSelector);
            await arrow.ClickAsync();

            await this.Page.WaitForAngular();

            var optionSelector = $"mat-option[data-allors-option-id='{@object?.Id ?? 0}'] span";
            var option = this.Page.Locator(optionSelector);
            await option.ClickAsync();
        }

        public async Task Toggle(params IObject[] objects)
        {
            await this.Page.WaitForAngular();

            var arrow = this.Page.Locator(this.ArrowSelector);
            await arrow.ClickAsync();

            foreach (var @object in objects)
            {
                await this.Page.WaitForAngular();

                var optionSelector = $"mat-option[data-allors-option-id='{@object.Id}'] span";
                var option = this.Page.Locator(optionSelector);
                await option.ClickAsync();
            }

            await this.Page.WaitForAngular();

            await this.Page.Keyboard.PressAsync("Escape");
        }
    }

    public class MatSelect<T> : MatSelect where T : Component
    {
        public MatSelect(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Toggle(params IObject[] objects)
        {
            await base.Toggle(objects);
            return this.Page;
        }

        public new async Task<T> Select(IObject @object)
        {
            await base.Select(@object);
            return this.Page;
        }
    }
}
