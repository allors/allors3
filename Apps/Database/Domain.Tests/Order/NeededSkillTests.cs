// <copyright file="NeededSkillTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class NeededSkillTests : DomainTest, IClassFixture<Fixture>
    {
        public NeededSkillTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenNeededSkill_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var projectManagement = new Skills(this.Transaction).ProjectManagement;
            var expert = new SkillLevels(this.Transaction).Expert;

            var builder = new NeededSkillBuilder(this.Transaction);
            var neededSkill = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithSkill(projectManagement);
            neededSkill = builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithSkillLevel(expert);
            neededSkill = builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
