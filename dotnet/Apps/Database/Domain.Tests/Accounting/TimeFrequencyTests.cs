// <copyright file="TimeFrequencyTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class TimeFrequencyTests : DomainTest, IClassFixture<Fixture>
    {
        public TimeFrequencyTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenTimeFrequencyWithoutConversionToTarget_WhenGetConvertToFactor_ThenReturnsNull()
        {
            var frequencies = new TimeFrequencies(this.Transaction);

            // Hour has conversions to Millisecond..Week but none to Year, so GetConvertToFactor must return null
            // rather than dereferencing the null FirstOrDefault result.
            Assert.Null(frequencies.Hour.GetConvertToFactor(frequencies.Year));
        }
    }
}
