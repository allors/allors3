// <copyright file="MatFactoryFab.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Factory
{
    using System.Threading.Tasks;
    using Database.Meta;
    using Html;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class FactoryFabComponent : IComponent
    {
        public FactoryFabComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("a-mat-factory-fab");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public Button Button => new Button(this, @"button[data-allors-objecttype]");

        public async Task<IComposite> ObjectType()
        {
            await this.Page.WaitForAngular();
            var tag = await this.Button.GetAttributeAsync("data-allors-objecttype");
            return (IComposite)this.M.FindByTag(tag);
        }

        public async Task Create(IClass @class = null)
        {
            await this.Page.WaitForAngular();
            await this.Button.ClickAsync();

            if (@class != null)
            {
                await this.Page.WaitForAngular();
                var button = new Button(this, $"button[data-allors-class='{@class.Tag}']");

                if (await button.IsVisibleAsync())
                {
                    await button.ClickAsync();
                }
            }
        }
    }
}
