// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SyncTests : DomainTest, IClassFixture<Fixture>
    {
        public SyncTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void IsSync()
        {
            var metaPopulation = this.Transaction.Database.MetaPopulation;
            foreach (var composite in metaPopulation.DatabaseComposites)
            {
                switch (composite.Name)
                {
                    case "SyncDepthI1":
                    case "SyncDepthC1":
                    case "SyncDepth2":
                        Assert.True(composite.IsSynced);
                        break;

                    default:
                        Assert.False(composite.IsSynced);
                        break;
                }
            }
        }
    }
}
