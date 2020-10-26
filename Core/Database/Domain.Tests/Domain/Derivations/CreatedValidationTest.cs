// <copyright file="CreatedValidationDomainDerivationTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests
{
    using Allors;
    using Allors.Domain;
    using Xunit;

    public class CreatedValidationDomainDerivationTest : DomainTest, IClassFixture<Fixture>
    {
        public CreatedValidationDomainDerivationTest(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void Create()
        {
            var aa = new AABuilder(this.Session)
                .Build();

            this.Session.Derive();

            Assert.True(aa.IsCreated);
        }
    }
}
