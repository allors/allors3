// <copyright file="BuilderTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Xunit;

    public class BuilderTest : DomainTest, IClassFixture<Fixture>
    {
        public BuilderTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ClassBuilder()
        {
            var build = new BuildBuilder(this.Transaction).Build();

            Assert.Equal(new Guid("DCE649A4-7CF6-48FA-93E4-CDE222DA2A94"), build.Guid);
            Assert.Equal("Exist", build.String);
        }


        [Fact]
        public void LambdaBuilder()
        {
            var build = this.Transaction.Build<Build>();

            Assert.Equal(new Guid("DCE649A4-7CF6-48FA-93E4-CDE222DA2A94"), build.Guid);
            Assert.Equal("Exist", build.String);
        }
    }
}
