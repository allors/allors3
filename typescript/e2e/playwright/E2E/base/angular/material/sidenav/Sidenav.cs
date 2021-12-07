// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Sidenav
{
    using Allors.Database.Meta;
    using Html;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public partial class Sidenav : IComponent
    {
        public Sidenav(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("mat-sidenav");
        }

        public IComponent AppRoot
        {
            get
            {
                var container = this.Container;
                while (container != null && !(container is AppRoot))
                {
                    container = container.Container;
                }

                return container;
            }
        }

        public IComponent Container { get; }

        public IPage Page => this.Container.Page;

        public MetaPopulation M => this.Container.M;

        public ILocator Locator { get; }

        public Button Toggle => new Button(this, @"button[aria-label=""Toggle sidenav""]");

        public async Task NavigateAsync(string link)
        {
            var anchor = this.Link(link);

            var isVisible = await anchor.IsVisibleAsync();
            if (!isVisible)
            {
                await this.Toggle.ClickAsync();
            }

            await anchor.ClickAsync();
        }

        public async Task NavigateAsync(string group, string link)
        {
            var element = this.Group(@group);
            var anchor = this.Link(link);

            var isVisible = await anchor.IsVisibleAsync();
            if (!isVisible)
            {
                var groupIsVisible = await element.IsVisibleAsync();
                if (!groupIsVisible)
                {
                    await this.Toggle.ClickAsync();
                }

                isVisible = await anchor.IsVisibleAsync();
                if (!isVisible)
                {
                    await element.ClickAsync();
                }
            }

            await anchor.ClickAsync();
        }

        private Element Group(string name) => new Element(this, $"span[contains(text(), '{name}')]");

        private Anchor Link(string href) => new Anchor(this, $"a[href='{href}']");
    }
}
