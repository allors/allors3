// <copyright file="MatMenu.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Menu
{
    using Allors.Database.Meta;
    using Cdk;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public partial class MatMenu : IComponent
    {
        public MatMenu(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator;
            this.OverlayContainer = new OverlayContainer(container);
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public OverlayContainer OverlayContainer { get; }

        public async Task Select(string value)
        {
            await this.Page.WaitForAngular();
            var arrow = this.OverlayContainer.Locator.Locator($"button[data-allors-action='{value}']");
            await arrow.ClickAsync();
        }
    }
}
