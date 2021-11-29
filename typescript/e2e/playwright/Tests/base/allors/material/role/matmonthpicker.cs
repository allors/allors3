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

    public class MatMonthpicker : SelectorComponent
    {
        public MatMonthpicker(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-monthpicker{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']//input";

        public override string Selector { get; }

        public async Task<DateTime?> GetValue()
        {
            await this.Page.WaitForAngular();

            var element = this.Page.Locator(this.Selector);
            var value = await element.GetAttributeAsync("value");
            if (!string.IsNullOrEmpty(value))
            {
                return DateTime.Parse(value);
            }

            return null;
        }

        public async Task SetValue(DateTime? value)
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.Selector);
            await element.FillAsync(value == null ? string.Empty : value.Value.ToString("d"));
        }
    }

    public class MatMonthpicker<T> : MatDatepicker where T : Component
    {
        public MatMonthpicker(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public async Task<T> Set(DateTime value)
        {
            await this.SetValue(value);
            return this.Page;
        }
    }
}
