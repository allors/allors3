// <copyright file="FilterTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Xunit;

    public class MetaBuilderTests
    {
        [Fact]
        public void Build()
        {
            var metaBuilder = new MetaBuilder();
            metaBuilder.Build();
        }

        // A dictionary or a set with types of the model as its keys asks a key at every lookup whether it
        // equals another one. An answer that puts the id of the other one in a box allocates 32 bytes for
        // each question, which adds up where the lookups are many, as in the extents of the adapters.
        [Fact]
        public void ComparingTypesOfTheModelDoesNotAllocate()
        {
            var meta = new MetaBuilder().Build();

            var kinds = new Dictionary<string, object[]>
            {
                ["domain"] = meta.Domains.Take(2).ToArray<object>(),
                ["inheritance"] = meta.Inheritances.Take(2).ToArray<object>(),
                ["unit"] = meta.Units.Take(2).ToArray<object>(),
                ["composite"] = meta.Composites.Take(2).ToArray<object>(),
                ["relation type"] = meta.RelationTypes.Take(2).ToArray<object>(),
                ["association type"] = meta.RelationTypes.Take(2).Select(v => v.AssociationType).ToArray<object>(),
                ["role type"] = meta.RelationTypes.Take(2).Select(v => v.RoleType).ToArray<object>(),
                ["method type"] = meta.MethodTypes.Take(2).ToArray<object>(),
            };

            var allocating = new List<string>();

            foreach (var kind in kinds)
            {
                Assert.NotEmpty(kind.Value);

                // With itself and with another one of its kind.
                var allocated = kind.Value.SelectMany(left => kind.Value.Select(right => Allocated(left, right))).Max();
                if (allocated != 0)
                {
                    allocating.Add($"{kind.Key}: {allocated} bytes in 1,000 comparisons");
                }
            }

            Assert.True(allocating.Count == 0, string.Join("; ", allocating));
        }

        private static long Allocated(object left, object right)
        {
            left.Equals(right);

            var before = GC.GetAllocatedBytesForCurrentThread();

            for (var i = 0; i < 1000; i++)
            {
                left.Equals(right);
            }

            return GC.GetAllocatedBytesForCurrentThread() - before;
        }
    }
}
