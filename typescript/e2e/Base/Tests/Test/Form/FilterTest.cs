// <copyright file="FilterTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Form
{
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Material.Filter;
    using Allors.E2E.Test;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class FilterTest : Test
    {
        public FilterComponent FilterComponent => new FilterComponent(this.AppRoot);

        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            await this.Page.NavigateAsync("/filter");
        }

        [Test]
        public async Task NonBetweenFieldDoesNotBecomeBetweenRange()
        {
            // Regression for the filter-field dialog: a value2 entered for a Between field must
            // not leak into a subsequently-selected non-Between field. Pick the Between (Decimal)
            // field and enter both ends, step back, pick the non-Between (String) field and enter
            // a single value, then apply. The String field must be saved as a single value, not a
            // Between range. (/filter is a test-only page that offers both field kinds.)
            var filter = this.FilterComponent;

            await filter.AddAsync();

            var dialog = new AllorsMaterialFilterFieldDialogComponent(this.OverlayContainer);

            await dialog.SelectFieldAsync("Decimal");
            await dialog.Value.SetAsync("10");
            await dialog.Value2.SetAsync("20");

            await dialog.BackAsync();

            await dialog.SelectFieldAsync("String");
            await dialog.Value.SetAsync("abc");

            await dialog.ApplyButton.ClickAsync();
            await this.Page.WaitForAngular();

            var chipText = await filter.Chips.First.TextContentAsync();

            // Without the fix, the stale "20" makes the String field a Between range ("abc <-> 20").
            ClassicAssert.IsFalse(
                chipText.Contains("<->"),
                $"Expected a single-value filter field, but got a Between range: '{chipText}'");
        }
    }
}
