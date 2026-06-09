// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Filter
{
    using Allors.Database.Meta;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public partial class FilterComponent : IComponent
    {
        public FilterComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("a-mat-filter");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        /// <summary>The applied filter chips (one per added field).</summary>
        public ILocator Chips => this.Locator.Locator("mat-chip");

        /// <summary>Clicks the filter toolbar to open the filter-field dialog.</summary>
        public async Task AddAsync()
        {
            await this.Page.WaitForAngular();
            await this.Locator.Locator("mat-toolbar").ClickAsync();
            await this.Page.WaitForAngular();
        }
    }
}
