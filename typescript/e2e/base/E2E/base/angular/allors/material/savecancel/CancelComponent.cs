// <copyright file="MatFactoryFab.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Factory
{
    using Database.Meta;
    using Html;
    using Info;
    using Microsoft.Playwright;

    public class CancelComponent : IComponent
    {
        public CancelComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("a-mat-cancel");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public Button Button => new Button(this, @"button");

        public async System.Threading.Tasks.Task CancelAsync()
        {
            await this.Page.WaitForAngular();
            await this.Button.ClickAsync();
        }
    }
}
