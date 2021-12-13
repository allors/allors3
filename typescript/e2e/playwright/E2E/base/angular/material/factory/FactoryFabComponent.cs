// <copyright file="MatFactoryFab.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Factory
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Html;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class FactoryFabComponent : IComponent
    {
        public FactoryFabComponent(IComponent container, IComposite composite)
        {
            this.Container = container;
            this.Composite = composite;
            this.Locator = this.Container.Locator.Locator("a-mat-factory-fab");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public IComposite Composite { get; set; }

        public Anchor Anchor => new Anchor(this, @"[mat-fab]");

        public Element MatMenu => new Element(this, @"mat-menu");

        public async Task Create(IClass @class = null)
        {
            await this.Anchor.ClickAsync();

            var classes = await this.Classes();
            if (@class != null && classes.Length > 1)
            {
                var button = new Button(this, $"button[data-allors-class='{@class.SingularName}']");
                await button.ClickAsync();
            }
        }

        public async Task<IClass[]> Classes()
        {
            var attribute = await this.MatMenu.GetAttributeAsync("data-allors-actions");
            var actions = !string.IsNullOrWhiteSpace(attribute) ? attribute.Split(",") : Array.Empty<string>();

            return this.Composite.Classes.Where(v => actions.Contains(v.SingularName)).Cast<IClass>().ToArray();
        }
    }
}
