// <copyright file="PartSpecificationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PartSpecificationTests : DomainTest, IClassFixture<Fixture>
    {
        public PartSpecificationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenConstraintSpecification_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var specification = new PartSpecificationBuilder(this.Transaction).WithDescription("specification").Build();

            this.Transaction.Derive();

            Assert.Equal(new PartSpecificationStates(this.Transaction).Created, specification.PartSpecificationState);
            Assert.Equal(specification.LastPartSpecificationState, specification.PartSpecificationState);
        }

        [Fact]
        public void GivenConstraintSpecification_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var specification = new PartSpecificationBuilder(this.Transaction).WithDescription("specification").Build();

            this.Transaction.Derive();

            Assert.Null(specification.PreviousPartSpecificationState);
        }
    }
}
