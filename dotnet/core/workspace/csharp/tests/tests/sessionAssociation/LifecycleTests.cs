// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.SessionAssociation
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using System;
    using Allors.Workspace.Data;

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
        public async void InstantiateOtherSession()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var objectSession1 = session1.Create<SC1>();

            var session2 = this.Workspace.CreateSession();

            var objectSession2 = session2.Instantiate(objectSession1);

            Assert.Null(objectSession2);
        }

        [Fact]
        public async void PullOtherSessionShouldThrowError()
        {
            var session1 = this.Workspace.CreateSession();

            var objectSession1 = session1.Create<SC1>();

            await this.AsyncDatabaseClient.PushAsync(session1);

            var session2 = this.Workspace.CreateSession();
            bool hasErrors;

            try
            {
                var result = await this.AsyncDatabaseClient.PullAsync(session2, new Pull { Object = objectSession1 });
                hasErrors = false;
            }
            catch (ArgumentException)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public void CrossSessionShouldThrowError()
        {
            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var objectSession1 = session1.Create<SC1>();
            var objectSession2 = session2.Create<SC1>();

            bool hasErrors;

            try
            {
                objectSession1.AddSessionSC1Many2Many(objectSession2);
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }
    }
}
