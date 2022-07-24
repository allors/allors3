// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Dynamic
{
    using Allors.Database.Meta;
    using Info;
    using Microsoft.Playwright;

    public partial class AllorsMaterialDynamicViewExtentPanelComponent : IComponent
    {
        public static string DataTag => "data-tag";

        public AllorsMaterialDynamicViewExtentPanelComponent(IComponent container, string dataTag)
        {
            this.Container = container;
            var locator = "a-mat-dyn-view-extent-panel";
            locator += $"[{DataTag}='{dataTag}']";
            this.Locator = this.Container.Locator.Locator(locator);
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;
    }
}
