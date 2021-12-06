// <copyright file="MatRadioGroup.cs" company="Allors bvba">
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

    public class MatRadioGroup : SelectorComponent
    {
        public MatRadioGroup(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page, m) =>
                this.Selector = $"a-mat-radio-group{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public override string Selector { get; }

        public async Task Select(string value)
        {
            await this.Page.WaitForAngular();
            var radioSelector = $"{this.Selector} mat-radio-button[data-allors-radio-value='{{value}}']";
            var radio = this.Page.Locator(radioSelector);
            await radio.ClickAsync();
        }
    }

    public class MatRadioGroup<T> : MatRadioGroup where T : Component
    {
        public MatRadioGroup(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Select(string value)
        {
            await base.Select(value);
            return this.Page;
        }
    }
}
