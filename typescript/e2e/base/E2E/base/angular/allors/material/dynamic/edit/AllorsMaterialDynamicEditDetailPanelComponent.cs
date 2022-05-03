// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Dynamic
{
    using Allors.Database.Meta;
    using Info;
    using Microsoft.Playwright;

    public partial class AllorsMaterialDynamicEditDetailPanelComponent : IComponent
    {
        public AllorsMaterialDynamicEditDetailPanelComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("a-mat-dyn-edit-detail-panel");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;
    }
}
