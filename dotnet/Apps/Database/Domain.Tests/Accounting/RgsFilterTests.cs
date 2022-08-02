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
        public void RgsFilterUseEzTrueAndExcludeEzFalseShouldNotGiveErrors()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseEz = true;
            rgsFilter.ExcludeEz = false;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
        }

        [Fact]
        public void RgsFilterUseEzFlaseAndExcludeEzTrueShouldNotGiveErrors()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseEz = false;
            rgsFilter.ExcludeEz = true;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
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
        public void RgsFilterUseZzpTrueAndExcludeZzpFalseShouldNotGiveErrors()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseZzp = true;
            rgsFilter.ExcludeZzp = false;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
        }

        [Fact]
        public void RgsFilterUseZzpFlaseAndExcludeZzpTrueShouldNotGiveErrors()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseZzp = false;
            rgsFilter.ExcludeZzp = true;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
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
        public void RgsFilterUseBaseTrueAndUseExtendedFalseShouldNotGiveError()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseBase = true;
            rgsFilter.UseExtended = false;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
        }

        [Fact]
        public void RgsFilterUseBaseFalseAndUseExtendedTrueShouldNotGiveError()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseBase = false;
            rgsFilter.UseExtended = true;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
        }

        [Fact]
        public void RgsFilterUseEZAndUseZzpAndUseWoCoWithAllTrueCanNotBeTwoTrue()
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
        public void RgsFilterUseEZAndUseZzpAndUseWoCoWithTwoTrueCanNotBeTwoTrue()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseEz = true;
            rgsFilter.UseZzp = false;
            rgsFilter.UseWoCo = true;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }

        [Fact]
        public void RgsFilterUseEZAndUseZzpAndUseWoCoWithOneTrueShouldNotGiveError()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseEz = true;
            rgsFilter.UseZzp = false;
            rgsFilter.UseWoCo = false;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
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

        [Fact]
        public void RgsFilterUseWoCoFalseAndUseExcludeWoCoTrueShouldNotGiveError()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseWoCo = false;
            rgsFilter.ExcludeWoCo = true;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
        }

        [Fact]
        public void RgsFilterUseWoCoTrueAndUseExcludeWoCoFalseShouldNotGiveError()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.UseWoCo = false;
            rgsFilter.ExcludeWoCo = true;

            errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);
        }

        [Fact]
        public void RgsFilterExcludeLevel5CanNotBeFalse()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.ExcludeLevel5 = false;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }

        [Fact]
        public void RgsFilterExcludeLevel5ExtensionCanNotBeFalse()
        {
            var rgsFilter = new RgsFilterBuilder(this.Transaction).Build();
            var errors = this.Derive().Errors.ToList();
            Assert.Empty(errors);

            rgsFilter.ExcludeLevel5Extension = false;

            errors = this.Derive().Errors.ToList();
            Assert.Single(errors);
        }
    }
}
