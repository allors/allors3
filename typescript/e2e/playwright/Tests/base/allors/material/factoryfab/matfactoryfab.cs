// <copyright file="MatFactoryFab.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class MatFactoryFab : SelectorComponent
    {
        public MatFactoryFab(IPage page, MetaPopulation m, Composite composite, string selector)
            : base(page, m)
        {
            this.Composite = composite;
            this.Selector = selector;
        }

        public Composite Composite { get; set; }

        public override string Selector { get; }

        public async Task Create(Class @class = null)
        {
            this.Anchor("[mat-fab]").Click();

            var classes = await this.Classes();
            if ((@class != null) && classes.Length > 1)
            {
                this.Button($"button[data-allors-class='{@class.Name}']").Click();
            }
        }

        private async Task<Class[]> Classes()
        {
            await this.Page.WaitForAngular();

            var classes = this.Composite.Classes;

            var fabPath = $"a-mat-factory-fab mat-menu";
            var path = this.Selector != null ? $"{this.Selector} {fabPath}" : fabPath;
            var fab = this.Page.Locator(path);
            var attribute = await fab.GetAttributeAsync("data-allors-actions");
            var actions = !string.IsNullOrWhiteSpace(attribute) ? attribute.Split(",") : Array.Empty<string>();

            return classes.Where(v => actions.Contains(v.SingularName)).Cast<Class>().ToArray();
        }
    }
}
