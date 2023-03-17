// <copyright file="BuilderTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class DeletingTest : DomainTest, IClassFixture<Fixture>
    {
        public DeletingTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void IsDeleting()
        {
            var cascader = new CascaderBuilder(this.Transaction)
                .WithCascaded(new CascadedBuilder(this.Transaction).Build())
                .Build();

            cascader.Delete();
        }
    }
}
