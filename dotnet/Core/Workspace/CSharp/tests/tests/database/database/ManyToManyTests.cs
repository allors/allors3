// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseDatabase
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using Allors.Workspace.Data;

    public abstract class ManyToManyTests : Test
    {
        private ISession session1;
        private C1 c1;
        private C2 c2;

        protected ManyToManyTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await this.Login("administrator");

            this.session1 = this.Workspace.CreateSession();

            this.c1 = this.session1.Create<C1>();
            this.c2 = this.session1.Create<C2>();
        }

        [Fact]
        public void SetRole_WithoutPush()
        {
            this.c1.AddC1C2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.C1C2Many2Manies);
        }

        [Fact]
        public async void SetRole_WithPush()
        {
            await this.session1.Push();

            #region pulls

            var pulls = new[]
            {
                new Pull
                {
                    Extent = new Filter(this.M.C1)
                },
                new Pull
                {
                    Extent = new Filter(this.M.C2)
                }
            };

            #endregion

            await this.session1.Pull(pulls);

            this.c1.AddC1C2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.C1C2Many2Manies);

            await this.session1.Push();
            await this.session1.Pull(pulls);

            Assert.Contains(this.c2, this.c1.C1C2Many2Manies);
        }

        [Fact]
        public void RemoveRole_WithoutPush()
        {
            this.c1.AddC1C2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.C1C2Many2Manies);

            this.c1.RemoveC1C2Many2Many(this.c2);

            Assert.DoesNotContain(this.c2, this.c1.C1C2Many2Manies);
        }

        [Fact]
        public async void RemoveRole_WithPush()
        {
            await this.session1.Push();

            #region pulls

            var pulls = new[]
            {
                new Pull
                {
                    Extent = new Filter(this.M.C1)
                },
                new Pull
                {
                    Extent = new Filter(this.M.C2)
                }
            };

            #endregion

            await this.session1.Pull(pulls);

            this.c1.AddC1C2Many2Many(this.c2);

            Assert.Contains(this.c2, this.c1.C1C2Many2Manies);

            await this.session1.Push();
            await this.session1.Pull(pulls);

            Assert.Contains(this.c2, this.c1.C1C2Many2Manies);

            this.c1.RemoveC1C2Many2Many(this.c2);

            Assert.DoesNotContain(this.c2, this.c1.C1C2Many2Manies);

            await this.session1.Push();

            Assert.DoesNotContain(this.c2, this.c1.C1C2Many2Manies);
        }
    }
}
