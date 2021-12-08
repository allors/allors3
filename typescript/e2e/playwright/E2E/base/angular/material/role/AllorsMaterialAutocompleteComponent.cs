// <copyright file="MatAutoComplete.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Material.Role
{
    using Database.Meta;
    using Microsoft.Playwright;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialAutocompleteComponent : RoleComponent
    {
        public AllorsMaterialAutocompleteComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-autocomplete")
        {
        }

        public ILocator InputLocator => this.Locator.Locator("input");

        public async Task SelectAsync(string value, string selection = null)
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
    }
}
