// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using Allors.Workspace.Data;
    using System;
    using static Names;

    public abstract class LifecycleTests : Test
    {
        protected LifecycleTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void PullSameSessionNotPushedException()
        {
            var session = this.Workspace.CreateSession();

            var c1 = session.Create<C1>();
            Assert.NotNull(c1);

            bool hasErrors;

            try
            {
                var result = await session.PullAsync(new Pull { Object = c1 });
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public async void PullOtherSessionNotPushedException()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            Assert.NotNull(c1);

            var session2 = this.Workspace.CreateSession();

            bool hasErrors;

            try
            {
                var result = await session2.PullAsync(new Pull { Object = c1 });
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public async Task RoleToObjectPulledByOtherSession()
        {
            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var c2c = (await session1.PullAsync(new Pull { Extent = new Filter(this.M.C2) { Predicate = new Equals(this.M.C2.Name) { Value = c2C } } })).GetCollection<C2>()[0];
            var c1c = (await session2.PullAsync(new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = c1C } } })).GetCollection<C1>()[0];

            var missing = Assert.Throws<InvalidOperationException>(() => session2.Instantiate<C2>(c2c.Id));
            Assert.Equal($"Object {c2c.Id} is not in this session. Pull it before instantiating it.", missing.Message);
            var exception = Assert.Throws<Exception>(() => c1c.C1C2One2One);
            Assert.Equal($"Object {c2c.Id} in role C1.C1C2One2One is not in this session. Include the role in the Pull, or pull the object, before reading it.", exception.Message);
        }
    }
}
