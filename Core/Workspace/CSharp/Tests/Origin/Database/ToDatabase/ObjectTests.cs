// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Database.ToDatabase
{
    using System.Net.Http;
    using Xunit;

    public class ObjectTests : Test
    {
        [Fact]
        public async void NonExistingPullController()
        {
            var session = this.Workspace.CreateSession();

            var exceptionThrown = false;
            try
            {
                await session.Load(new { step = 0 }, "ThisIsWrong");
            }
            catch (HttpRequestException)
            {
                exceptionThrown = true;
            }

            Assert.True(exceptionThrown);
        }
    }
}
