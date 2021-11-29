// <copyright file="MatTextarea.cs" company="Allors bvba">
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

    public class MatLocalisedText : SelectorComponent
    {
        public MatLocalisedText(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-localised-text{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public override string Selector { get; }

        public async Task<string> GetValue()
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.Selector);
            return await element.GetAttributeAsync("value");
        }

        public async Task SetValue(string value)
        {
            await this.Page.WaitForAngular();
            var element = this.Page.Locator(this.Selector);
            await element.FillAsync(value);
        }
    }


    public class MatLocalisedText<T> : MatLocalisedText where T : Component
    {
        public MatLocalisedText(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> SetValue(string value)
        {
            await base.SetValue(value);
            return this.Page;
        }
    }
}
