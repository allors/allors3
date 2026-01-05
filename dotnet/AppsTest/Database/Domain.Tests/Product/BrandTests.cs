// <copyright file="PersonTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class BrandDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public BrandDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).BrandDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedBrandDeriveDeletePermission()
        {
            var brand = new BrandBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, brand.Revocations);
        }

        [Fact]
        public void OnChangedPartWhereBrandDeriveDeletePermission()
        {
            var brand = new BrandBuilder(this.Transaction).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            part.Brand = brand;
            this.Derive();

            Assert.Contains(this.deleteRevocation, brand.Revocations);
        }
    }
}
