// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Sidenav
{
    using Allors.Database.Meta;
    using Html;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public partial class Sidenav : IComponent
    {
        public Sidenav(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("mat-sidenav-container");
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

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public Button Toggle => new Button(this, @"button[aria-label=""Toggle sidenav""] >> nth=0");

        public async Task NavigateAsync(ComponentInfo componentInfo)
        {
            var menuInfo = componentInfo.MenuInfo;
            var link = menuInfo.Link ?? componentInfo.RouteInfo.FullPath;
            var anchor = new Anchor(this, $"a[href='{link}']");

            var parentMenuInfo = menuInfo.Parent;
            if (parentMenuInfo != null)
            {
                var span = new Element(this, @$"span.mat-line:text-is(""{parentMenuInfo.Title}"")");

                await this.Page.WaitForAngular();
                if (!await span.IsVisibleAsync())
                {
                    await this.Toggle.ClickAsync();
                }

                await this.Page.WaitForAngular();
                if (!await anchor.IsVisibleAsync())
                {
                    await span.ClickAsync();
                }
            }

            await this.Page.WaitForAngular();
            if (!await anchor.IsVisibleAsync())
            {
                await this.Toggle.ClickAsync();
            }

            await this.Page.WaitForAngular();
            await anchor.ClickAsync();
        }
    }
}
