// <copyright file="SalesOrderTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SalesOrderTransferTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderTransferTests(Fixture fixture) : base(fixture) { }
    }

    [Trait("Category", "Security")]
    public class SalesOrderTransferSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderTransferSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };
    }
}
