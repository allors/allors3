// <copyright file="MaterialsUsageTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class MaterialsUsageTests : DomainTest, IClassFixture<Fixture>
    {
        public MaterialsUsageTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var basePrice = new MaterialsUsageBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("MaterialsUsage.WorkEffort, MaterialsUsage.EngagementItem at least one"));
        }
    }
}
