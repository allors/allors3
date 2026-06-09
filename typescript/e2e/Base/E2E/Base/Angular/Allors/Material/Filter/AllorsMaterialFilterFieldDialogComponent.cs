// <copyright file="AllorsMaterialFilterFieldDialogComponent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Filter
{
    using Allors.Database.Meta;
    using Html;
    using Info;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The filter-field dialog (a vertical stepper: step 0 picks the field, step 1 enters the
    /// value(s)). Rendered in the CDK overlay, so construct it with the overlay container.
    /// </summary>
    public class AllorsMaterialFilterFieldDialogComponent : IComponent
    {
        public AllorsMaterialFilterFieldDialogComponent(IComponent container)
        {
            this.Container = container;
            this.Locator = this.Container.Locator.Locator("mat-dialog-container");
        }

        public IComponent Container { get; }

        public MetaPopulation M => this.Container.M;

        public IPage Page => this.Container.Page;

        public ILocator Locator { get; }

        public ApplicationInfo ApplicationInfo => this.Container.ApplicationInfo;

        public Input Value => new Input(this, "input[formcontrolname='value']");

        public Input Value2 => new Input(this, "input[formcontrolname='value2']");

        public Button ApplyButton => new Button(this, "button:has-text('Apply')");

        /// <summary>Picks a field definition by its (humanized) name on the first stepper step.</summary>
        public async Task SelectFieldAsync(string name)
        {
            await this.Page.WaitForAngular();
            await new Button(this, $"button:has-text('{name}')").ClickAsync();
            await this.Page.WaitForAngular();
        }

        /// <summary>Returns to the field-selection step by clicking the first step header.</summary>
        public async Task BackAsync()
        {
            await this.Page.WaitForAngular();
            await this.Locator.Locator(".mat-step-header").First.ClickAsync();
            await this.Page.WaitForAngular();
        }
    }
}
