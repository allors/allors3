// <copyright file="WorkEffortTypeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class WorkEffortTypeTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTypeTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortPartStandardsDeriveCurrentWorkEffortPartStandards()
        {
            var partStandard = new WorkEffortPartStandardBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Session).Build();
            this.Session.Derive(false);

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Session.Derive(false);

            Assert.Contains(partStandard, workEffortType.CurrentWorkEffortPartStandards);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardFromDateDeriveCurrentWorkEffortPartStandards()
        {
            var partStandard = new WorkEffortPartStandardBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Session).Build();
            this.Session.Derive(false);

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Session.Derive(false);

            Assert.Contains(partStandard, workEffortType.CurrentWorkEffortPartStandards);

            partStandard.FromDate = this.Session.Now().AddDays(1);
            this.Session.Derive(false);

            Assert.DoesNotContain(partStandard, workEffortType.CurrentWorkEffortPartStandards);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardThroughDateDeriveCurrentWorkEffortPartStandards()
        {
            var partStandard = new WorkEffortPartStandardBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Session).Build();
            this.Session.Derive(false);

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Session.Derive(false);

            Assert.Contains(partStandard, workEffortType.CurrentWorkEffortPartStandards);

            partStandard.ThroughDate = partStandard.FromDate;
            this.Session.Derive(false);

            Assert.DoesNotContain(partStandard, workEffortType.CurrentWorkEffortPartStandards);
        }
    }
}
