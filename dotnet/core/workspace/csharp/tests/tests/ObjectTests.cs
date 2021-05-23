// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Collections.Generic;
    using Allors.Workspace.Data;
    using Xunit;

    public abstract class ObjectTests : Test
    {
        protected ObjectTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void NonExistingPullService()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var exceptionThrown = false;
            try
            {
                var procedure = new Procedure("ThisIsWrong")
                {
                    Values = new Dictionary<string, string> { { "step", "0" } }
                };

                _ = await session.Pull(procedure);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            Assert.True(exceptionThrown);
        }
    }
}
