// <copyright file="UnitSamplesTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;

    using Xunit;

    public abstract class UnitTests : Test
    {
        [Fact]
        public async void Load()
        {
            var pull = new[]
            {
                new Pull
                {
                    Extent = new Extent(this.M.C1.ObjectType)
                },
            };

            var session = this.Workspace.CreateSession();
            var result = await session.Load(pull);

            var c1s = result.GetCollection<C1>();

            var c1A = c1s.First(v => v.Name.Equals("c1A"));
            var c1B = c1s.First(v => v.Name.Equals("c1B"));
            var c1C = c1s.First(v => v.Name.Equals("c1C"));
            var c1D = c1s.First(v => v.Name.Equals("c1D"));

            Assert.Equal("ᴀbra", c1B.C1AllorsString);
        }
    }
}