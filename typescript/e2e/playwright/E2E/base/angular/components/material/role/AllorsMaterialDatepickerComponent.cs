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
    using DateTime = System.DateTime;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialDatepickerComponent : RoleControl
    {
        public AllorsMaterialDatepickerComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-datepicker")
        {
        }

        public ILocator InputLocator => this.Locator.Locator("input");

        public async Task<DateTime?> GetAsync()
        {
            await this.Page.WaitForAngular();

            var attributeValue = await this.InputLocator.InputValueAsync();
            if (string.IsNullOrWhiteSpace(attributeValue))
            {
                return null;
            }

            return DateTime.Parse(attributeValue);
        }

        public async Task SetAsync(DateTime? value) => await this.InputLocator.FillAsync(value.HasValue ? value.Value.ToString("d") : string.Empty);
    }
}
