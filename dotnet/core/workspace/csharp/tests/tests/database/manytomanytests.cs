// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Database
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using Allors.Workspace.Data;

    public abstract class ManyToManyTests : Test
    {
        protected ManyToManyTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void SetRole()
        {
            // Single session
            #region No push before add
            {
                var session = this.Workspace.CreateSession();

                var c1a = session.Create<C1>();
                var c1b = session.Create<C1>();

                c1a.AddC1C1Many2Many(c1b);

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PullAsync(new Pull { Object = c1a });

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
            }

            {
                var session = this.Workspace.CreateSession();

                var c1a = session.Create<C1>();
                var c1b = session.Create<C1>();

                c1a.AddC1C1Many2Many(c1b);

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PullAsync(new Pull { Object = c1b });

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
            }

            {
                var session = this.Workspace.CreateSession();

                var c1a = session.Create<C1>();
                var c1b = session.Create<C1>();

                c1a.AddC1C1Many2Many(c1b);

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PullAsync(new Pull { Object = c1a }, new Pull { Object = c1b });

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
            }
            #endregion

            #region Push c1a to database before add
            {
                var session = this.Workspace.CreateSession();

                var c1a = session.Create<C1>();

                await session.PushAsync();

                var c1b = session.Create<C1>();

                Assert.False(c1a.CanWriteC1C1Many2Manies);
                c1a.AddC1C1Many2Many(c1b);

                Assert.Empty(c1a.C1C1Many2Manies);
                Assert.Empty(c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Empty(c1a.C1C1Many2Manies);
                Assert.Empty(c1b.C1sWhereC1C1Many2Many);
            }
            #endregion

            #region Push/Pull c1a to database before add
            {
                var session = this.Workspace.CreateSession();

                var c1a = session.Create<C1>();

                await session.PushAsync();
                await session.PullAsync(new Pull { Object = c1a });

                var c1b = session.Create<C1>();

                c1a.AddC1C1Many2Many(c1b);

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
            }
            #endregion

            #region Push c1b to database before add
            {
                var session = this.Workspace.CreateSession();

                var c1b = session.Create<C1>();

                await session.PushAsync();

                var c1a = session.Create<C1>();

                c1a.AddC1C1Many2Many(c1b);

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
            }
            #endregion

            #region Push c1a and c1b to database before add
            {
                var session = this.Workspace.CreateSession();

                var c1a = session.Create<C1>();
                var c1b = session.Create<C1>();

                await session.PushAsync();

                Assert.False(c1a.CanWriteC1C1Many2Manies);
                c1a.AddC1C1Many2Many(c1b);

                Assert.Empty(c1a.C1C1Many2Manies);
                Assert.Empty(c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Empty(c1a.C1C1Many2Manies);
                Assert.Empty(c1b.C1sWhereC1C1Many2Many);
            }
            #endregion

            #region Push/Pull c1a and c1b to database before add
            {
                var session = this.Workspace.CreateSession();

                var c1a = session.Create<C1>();
                var c1b = session.Create<C1>();

                await session.PushAsync();
                await session.PullAsync(new Pull { Object = c1a }, new Pull { Object = c1b });

                c1a.AddC1C1Many2Many(c1b);

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

                await session.PushAsync();

                Assert.Single(c1a.C1C1Many2Manies);
                Assert.Single(c1b.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
            }
            #endregion

            // Multiple Sessions
            #region c1a in other session
            {
                var session1 = this.Workspace.CreateSession();
                var session2 = this.Workspace.CreateSession();

                var c1a_2 = session2.Create<C1>();
                var c1b_1 = session1.Create<C1>();

                await session2.PushAsync();
                await session1.PullAsync(new Pull { Object = c1a_2 });

                var c1a_1 = session1.Instantiate(c1a_2);

                c1a_1.AddC1C1Many2Many(c1b_1);

                Assert.Single(c1a_1.C1C1Many2Manies);
                Assert.Single(c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);

                await session1.PushAsync();

                Assert.Single(c1a_1.C1C1Many2Manies);
                Assert.Single(c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
            }
            #endregion
            #region c1b in other session
            {
                var session1 = this.Workspace.CreateSession();
                var session2 = this.Workspace.CreateSession();

                var c1a_1 = session1.Create<C1>();
                var c1b_2 = session2.Create<C1>();

                await session2.PushAsync();
                await session1.PullAsync(new Pull { Object = c1b_2 });

                var c1b_1 = session1.Instantiate(c1b_2);

                c1a_1.AddC1C1Many2Many(c1b_1);

                Assert.Single(c1a_1.C1C1Many2Manies);
                Assert.Single(c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);

                await session1.PushAsync();

                Assert.Single(c1a_1.C1C1Many2Manies);
                Assert.Single(c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
            }
            #endregion
            #region c1a and c1b in other session
            {
                var session1 = this.Workspace.CreateSession();
                var session2 = this.Workspace.CreateSession();

                var c1a_2 = session2.Create<C1>();
                var c1b_2 = session2.Create<C1>();

                await session2.PushAsync();
                await session1.PullAsync(new Pull { Object = c1a_2 }, new Pull { Object = c1b_2 });

                var c1a_1 = session1.Instantiate(c1a_2);
                var c1b_1 = session1.Instantiate(c1b_2);

                c1a_1.AddC1C1Many2Many(c1b_1);

                Assert.Single(c1a_1.C1C1Many2Manies);
                Assert.Single(c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);

                await session1.PushAsync();

                Assert.Single(c1a_1.C1C1Many2Manies);
                Assert.Single(c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
                Assert.Contains(c1a_1, c1b_1.C1sWhereC1C1Many2Many);
            }
            #endregion
        }
    }
}
