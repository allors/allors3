// <copyright file="MatDatePicker.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using DateTime = System.DateTime;
    using Task = System.Threading.Tasks.Task;

    public class MatDatepicker
    : SelectorComponent
    {
        public MatDatepicker(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-datepicker{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}'] input";

        public override string Selector { get; }

        private ILocator Element => this.Page.Locator(this.Selector);

        public async Task<DateTime?> GetValue()
        {
            await this.Page.WaitForAngular();

            var value = await this.Element.InputValueAsync();
            if (!string.IsNullOrEmpty(value))
            {
                return DateTime.Parse(value);
            }

            return null;
        }

        public async Task SetValue(DateTime? value)
        {
            await this.Page.WaitForAngular();

            await this.Element.FillAsync(value == null ? string.Empty : value.Value.ToString("d"));

            await this.Page.WaitForAngular();

            await this.Page.Keyboard.PressAsync("Tab");
        }
    }

    public class MatDatepicker<T> : MatDatepicker where T : Component
    {
        public MatDatepicker(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> SetValue(DateTime? value)
        {
            await base.SetValue(value);
            return this.Page;
        }
    }
}
