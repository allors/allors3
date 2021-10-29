// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Collections.Generic;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class ProcedureTests : Test
    {
        protected ProcedureTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void TestUnitSamplesWithNulls()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var procedure = new Procedure("TestUnitSamples")
            {
                Values = new Dictionary<string, string> { { "step", "0" } }
            };

            var result = await session.CallAsync(procedure);

            Assert.False(result.HasErrors);

            var unitSample = result.GetObject<UnitSample>("unitSample");

            Assert.False(unitSample.ExistAllorsBinary);
            Assert.False(unitSample.ExistAllorsBoolean);
            Assert.False(unitSample.ExistAllorsDateTime);
            Assert.False(unitSample.ExistAllorsDecimal);
            Assert.False(unitSample.ExistAllorsDouble);
            Assert.False(unitSample.ExistAllorsInteger);
            Assert.False(unitSample.ExistAllorsString);
            Assert.False(unitSample.ExistAllorsUnique);
        }

        [Fact]
        public async void TestUnitSamplesWithValues()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var procedure = new Procedure("TestUnitSamples")
            {
                Values = new Dictionary<string, string> { { "step", "1" } }
            };

            var result = await session.CallAsync(procedure);

            Assert.False(result.HasErrors);

            var unitSample = result.GetObject<UnitSample>("unitSample");

            Assert.True(unitSample.ExistAllorsBinary);
            Assert.True(unitSample.ExistAllorsBoolean);
            Assert.True(unitSample.ExistAllorsDateTime);
            Assert.True(unitSample.ExistAllorsDecimal);
            Assert.True(unitSample.ExistAllorsDouble);
            Assert.True(unitSample.ExistAllorsInteger);
            Assert.True(unitSample.ExistAllorsString);
            Assert.True(unitSample.ExistAllorsUnique);

            Assert.Equal(new byte[] { 1, 2, 3 }, unitSample.AllorsBinary);
            Assert.True(unitSample.AllorsBoolean);
            Assert.Equal(new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc), unitSample.AllorsDateTime);
            Assert.Equal(12.34m, unitSample.AllorsDecimal);
            Assert.Equal(123d, unitSample.AllorsDouble);
            Assert.Equal(1000, unitSample.AllorsInteger);
            Assert.Equal("a string", unitSample.AllorsString);
            Assert.Equal(new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF"), unitSample.AllorsUnique);
        }

        [Fact]
        public async void NonExistingProcedure()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var procedure = new Procedure("ThisIsWrong")
            {
                Values = new Dictionary<string, string> { { "step", "0" } }
            };

            var result = await session.CallAsync(procedure);

            Assert.True(result.HasErrors);
        }
    }
}
