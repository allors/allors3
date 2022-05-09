// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Sidenav
{
    using Allors.Database.Meta;
    using Html;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public partial class AllorsMaterialDialogComponent : IComponent
    {
        public AllorsMaterialDialogComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("mat-dialog-container");
        }

        public IComponent AppRoot
        {
            get
            {
                var container = this.Container;
                while (container != null && !(container is AppRoot))
                {
                    container = container.Container;
                }

                return container;
            }
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;
        
        public Input Input => new Input(this, "");

        public Button CloseButton => new Button(this, @"button:has-text('Close')");

        public Button YesButton => new Button(this, @"button:has-text('Yes')");

        public Button NoButton => new Button(this, @"button:has-text('No')");

        public Button OkButton => new Button(this, @"button:has-text('Ok')");

        public Button CancelButton => new Button(this, @"button:has-text('Cancel')");

    }
}
