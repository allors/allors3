// <copyright file="MatMenu.cs" company="Allors bvba">
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

    public class MatMenu : SelectorComponent
    {
        public MatMenu(IPage page, MetaPopulation m, params string[] scopes) : base(page, m) => this.Selector = "button[data-allors-action]";

        public override string Selector { get; }

        public async Task Select(string value)
        {
            await this.Page.WaitForAngular();
            var arrow = this.Page.Locator($"button[data-allors-action='{value}']");
            await arrow.ClickAsync();
        }
    }

    public class MatMenu<T> : MatMenu where T : Component
    {
        public MatMenu(T page, MetaPopulation m, params string[] scopes)
            : base(page.Page, m, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Select(string value)
        {
            await base.Select(value);
            return this.Page;
        }
    }
}
