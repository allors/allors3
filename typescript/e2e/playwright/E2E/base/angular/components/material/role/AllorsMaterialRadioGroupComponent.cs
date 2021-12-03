// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialRadioGroupComponent : RoleControl
    {
        public AllorsMaterialRadioGroupComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-radio-group")
        {
        }

        public async Task SelectAsync(string value)
        {
            await this.Page.WaitForAngular();

            var isVisible = await this.Locator.IsVisibleAsync();

            var radio = this.Locator.Locator($"mat-radio-button[data-allors-radio-value='{value}']");
            await radio.ClickAsync();
        }
    }
}
