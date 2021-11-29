// <copyright file="Element.cs" company="Allors bvba">
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

    public class Element : SelectorComponent
    {
        public Element(IPage page, MetaPopulation m, string selector)
        : base(page, m) =>
            this.Selector = selector;

        public override string Selector { get; }

        public async Task<bool> IsVisible() => await this.Page.IsVisibleAsync(this.Selector);

        public async Task Click()
        {
            await this.Page.WaitForAngular();

            var element = this.Page.Locator(this.Selector);
            await element.ClickAsync();
        }
    }


    public class Element<T> : Element where T : Component
    {
        public Element(T page, MetaPopulation m, string selector)
            : base(page.Page, m, selector) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Click()
        {
            await base.Click();
            return this.Page;
        }
    }
}
