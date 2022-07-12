// <copyright file="JournalTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Resources;
    using Xunit;

    public class RgsFilterTests : DomainTest, IClassFixture<Fixture>
    {
        public RgsFilterTests(Fixture fixture) : base(fixture) { }
    }

    public class RgsFilterRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public RgsFilterRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void RgsFilterUseEzAndExcludeEzCanNotBeBothTrue()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseEz = true;
            rgsFilter.ExcludeEz = true;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }

        [Fact]
        public void RgsFilterUseZzpAndExcludeZzpCanNotBeBothTrue()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseZzp = true;
            rgsFilter.ExcludeZzp = true;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }

        [Fact]
        public void RgsFilterUseBaseAndUseExtendedCanNotBeBothTrue()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseBase = true;
            rgsFilter.UseExtended = true;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }

        [Fact]
        public void RgsFilterUseEZAndUseZzpAndUseWoCoCanNotAllBeTrue()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseEz = true;
            rgsFilter.UseZzp = true;
            rgsFilter.UseWoCo = true;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }

        [Fact]
        public void RgsFilterUseWoCoAndUseExcludeWoCoCanNotBeBothTrue()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseWoCo = true;
            rgsFilter.ExcludeWoCo = true;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }
    }
}
