// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Allors.Database.Meta;
    using Components;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public partial class Sidenav : SelectorComponent
    {
        public Sidenav(IPage page, MetaPopulation m)
        : base(page, m) =>
            this.Selector = "mat-sidenav";

        public override string Selector { get; }

        public Button Toggle => new Button(this.Page, this.M, @"button[aria-label=""Toggle sidenav""]");

        private async Task Navigate(Anchor link)
        {
            await this.Page.WaitForAngular();

            var isVisible = await link.IsVisible();
            if (!isVisible)
            {
                await this.Toggle.Click();
            }

            await link.Click();
        }

        private async Task Navigate(Element group, Anchor link)
        {
            await this.Page.WaitForAngular();

            var isVisible = await link.IsVisible();
            if (!isVisible)
            {
                var groupIsVisible = await group.IsVisible();
                if (!groupIsVisible)
                {
                    await this.Toggle.Click();
                    await this.Page.WaitForAngular();
                }

                isVisible = await link.IsVisible();
                if (!isVisible)
                {
                    await group.Click();
                }
            }

            await link.Click();
        }

        private Element Group(string name) => new Element(this.Page, this.M, this.Selector + " " + $".//span[contains(text(), '{name}')]");

        private Anchor Link(string href) => new Anchor(this.Page, this.M, this.ByHref(href));

        private string ByHref(string href) => this.Selector + " " + $"a[href='{href}']";
    }
}
