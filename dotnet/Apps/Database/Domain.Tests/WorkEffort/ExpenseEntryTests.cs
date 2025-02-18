// <copyright file="ExpenseEntryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class ExpenseEntryTests : DomainTest, IClassFixture<Fixture>
    {
        public ExpenseEntryTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var basePrice = new ExpenseEntryBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("ExpenseEntry.WorkEffort, ExpenseEntry.EngagementItem at least one"));
        }
    }
}
