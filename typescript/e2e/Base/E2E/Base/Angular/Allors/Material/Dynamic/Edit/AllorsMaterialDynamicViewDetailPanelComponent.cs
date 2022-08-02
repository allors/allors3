// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Dynamic
{
    using Database.Meta;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public partial class AllorsMaterialDynamicViewDetailPanelComponent : IComponent
    {
        public AllorsMaterialDynamicViewDetailPanelComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("a-mat-dyn-view-detail-panel");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public async Task ClickAsync() => await this.Locator.ClickAsync();
    }
}
