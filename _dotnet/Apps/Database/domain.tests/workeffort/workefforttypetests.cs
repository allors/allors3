// <copyright file="WorkEffortTypeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class WorkEffortTypeTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTypeTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortPartStandardsDeriveCurrentWorkEffortPartStandards()
        {
            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Derive();

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Derive();

            Assert.Contains(partStandard, workEffortType.CurrentWorkEffortPartStandards);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardFromDateDeriveCurrentWorkEffortPartStandards()
        {
            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Derive();

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Derive();

            Assert.Contains(partStandard, workEffortType.CurrentWorkEffortPartStandards);

            partStandard.FromDate = this.Transaction.Now().AddDays(1);
            this.Derive();

            Assert.DoesNotContain(partStandard, workEffortType.CurrentWorkEffortPartStandards);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardThroughDateDeriveCurrentWorkEffortPartStandards()
        {
            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Derive();

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Derive();

            Assert.Contains(partStandard, workEffortType.CurrentWorkEffortPartStandards);

            partStandard.ThroughDate = partStandard.FromDate;
            this.Derive();

            Assert.DoesNotContain(partStandard, workEffortType.CurrentWorkEffortPartStandards);
        }
    }
}
