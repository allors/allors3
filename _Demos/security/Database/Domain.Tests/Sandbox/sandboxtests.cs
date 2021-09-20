// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SandboxTests : DomainTest, IClassFixture<Fixture>
    {
        public SandboxTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Dummy()
        {
            // arrange

            // act

            // assert
        }
    }
}
