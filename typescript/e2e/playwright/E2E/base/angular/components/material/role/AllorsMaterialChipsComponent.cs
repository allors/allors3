// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialChipsComponent : RoleControl
    {
        public AllorsMaterialChipsComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-chips")
        {
        }

        public ILocator InputLocator => this.Locator.Locator("input");

        public async Task AddAsync(string value, string selection = null)
        {
            await this.Page.WaitForAngular();

            await this.InputLocator.FillAsync(string.Empty);

            await this.Page.WaitForAngular();

            await this.InputLocator.TypeAsync(value);

            await this.Page.WaitForAngular();

            value = this.CssEscape(value);
            var optionLocator = this.Page.Locator($"mat-option[data-allors-option-display='{selection ?? value}'] span");
            await optionLocator.ClickAsync();
        }

        public async Task RemoveAsync(string value)
        {
            await this.Page.WaitForAngular();

            value = this.CssEscape(value);
            var chipLocator = this.Locator.Locator($"mat-chip[data-allors-chip-display='{value}'] mat-icon");
            await chipLocator.ClickAsync();
        }
    }
}
