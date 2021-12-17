// <copyright file="Input.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Cdk
{
    using Database.Meta;
    using Info;
    using Microsoft.Playwright;

    public class OverlayContainer : IComponent
    {
        public OverlayContainer(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Page.Locator("div.cdk-overlay-container");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;
    }
}
