// <copyright file="MatSlideToggle.cs" company="Allors bvba">
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

    public class MatSlideToggle : SelectorComponent
    {
        public MatSlideToggle(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m)
        {
            this.Selector = $"mat-slide-toggle[@data-allors-roletype='{roleType.RelationType.Tag}'{this.ByScopesAnd(scopes)}]";
            this.InputSelector = $"{this.Selector} input";
        }

        public override string Selector { get; }

        public string InputSelector { get; }

        public async Task<bool> GetValue()
        {
            await this.Page.WaitForAngular();
            return await this.Page.IsCheckedAsync(this.InputSelector);
        }

        public async Task SetValue(bool value)
        {
            await this.Page.WaitForAngular();

            var container = this.Page.Locator(this.Selector);
            var element = this.Page.Locator(this.InputSelector);

            var isSelected = await this.GetValue();
            if (isSelected)
            {
                if (!value)
                {
                    await element.SelectTextAsync();
                }
            }
            else if (value)
            {
                await element.SelectTextAsync();
            }
        }
    }


    public class MatSlidetoggle<T> : MatSlideToggle where T : Component
    {
        public MatSlidetoggle(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
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
