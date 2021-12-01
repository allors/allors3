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

    public class AllorsMaterialDatetimepickerComponent : RoleControl
    {
        public AllorsMaterialDatetimepickerComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-datetimepicker")
        {
        }

        public ILocator InputLocator => this.Locator.Locator("input");

        public ILocator DateLocator => this.InputLocator.Nth(0);

        public ILocator HourLocator => this.InputLocator.Nth(1);

        public ILocator MinuteLocator => this.InputLocator.Nth(2);

        public async Task<DateTime?> GetAsync()
        {
            await this.Page.WaitForAngular();

            var attributeValue = await this.DateLocator.GetAttributeAsync("value");
            if (string.IsNullOrWhiteSpace(attributeValue))
            {
                return null;
            }

            var dateValue = DateTime.Parse(attributeValue);

            var hourValue = await this.HourLocator.GetAttributeAsync("value");
            int.TryParse(hourValue, out var hours);

            var minuteValue = await this.MinuteLocator.GetAttributeAsync("value");
            int.TryParse(minuteValue, out var minutes);

            return new DateTime(dateValue.Year, dateValue.Month, dateValue.Day, hours, minutes, 0);
        }

        public async Task SetAsync(DateTime? value)
        {
            await this.DateLocator.FillAsync(value.HasValue ? value.Value.ToString("d") : string.Empty);

            await this.Page.Keyboard.PressAsync("Tab");

            await this.HourLocator.FillAsync(value.HasValue ? value.Value.Hour.ToString() : string.Empty);
            await this.MinuteLocator.FillAsync(value.HasValue ? value.Value.Minute.ToString() : string.Empty);
        }
    }
}
