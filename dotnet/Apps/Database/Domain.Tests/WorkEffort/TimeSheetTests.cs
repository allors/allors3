// <copyright file="TimeSheetTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class TimeSheetTests : DomainTest, IClassFixture<Fixture>
    {
        public TimeSheetTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenTimeSheet_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            // Arrange
            var timeSheet = new TimeSheetBuilder(this.Transaction).Build();

            // Act
            var derivation = this.Derive();
            var originalCount = derivation.Errors.Count();

            // Assert
            Assert.True(derivation.HasErrors);

            //// Re-arrange
            var worker = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Worker").Build();
            timeSheet.Worker = worker;

            // Act
            derivation = this.Derive();

            // Assert
            Assert.False(derivation.HasErrors);
        }
    }
}
