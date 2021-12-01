// <copyright file="MatDatetimePicker.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using DateTime = System.DateTime;
    using Task = System.Threading.Tasks.Task;

    public class MatDatetimepicker : SelectorComponent
    {
        public MatDatetimepicker(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(page, m) =>
            this.Selector = $"a-mat-datetimepicker{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']//input";

        public override string Selector { get; }

        private ILocator DateElement => this.Page.Locator(this.Selector).First;

        private ILocator MinuteElement => this.Page.Locator(this.Selector).Nth(2);

        private ILocator HourElement => this.Page.Locator(this.Selector).Nth(1);

        public async Task<DateTime?> GetValue()
        {
            await this.Page.WaitForAngular();

            var dateElement = this.DateElement;
            var dateValue = await dateElement.InputValueAsync();

            if (!string.IsNullOrEmpty(dateValue))
            {
                var hourValue = await this.HourElement.InputValueAsync();
                int.TryParse(hourValue, out var hours);

                var minuteValue = await this.MinuteElement.InputValueAsync();
                int.TryParse(minuteValue, out var minutes);

                if (DateTime.TryParse(dateValue, out var date))
                {
                    var dateTime = new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);
                    return dateTime.ToUniversalTime();
                }
            }

            return null;
        }

        public async Task SetValue(DateTime? value)
        {
            value = value?.ToLocalTime();

            await this.Page.WaitForAngular();

            await this.DateElement.FillAsync(value == null ? string.Empty : value.Value.ToString("d"));
            await this.HourElement.FillAsync(value == null ? string.Empty : value.Value.Hour.ToString());
            await this.MinuteElement.FillAsync(value == null ? string.Empty : value.Value.Minute.ToString());
        }
    }

    public class MatDatetimepicker<T> : MatDatetimepicker where T : Component
    {
        public MatDatetimepicker(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> SetValue(DateTime? value)
        {
            await base.SetValue(value);
            return this.Page;
        }
    }
}
