// <copyright file="SandboxTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the SandboxTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SandboxTests : DomainTest, IClassFixture<Fixture>
    {
        public SandboxTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Dummy()
        {
        }
    }
}
