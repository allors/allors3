// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Adapters
{
    using System.Net.Http;
    using Nito.AsyncEx;

    using Xunit;

    public class ObjectTests : Test
    {
        [Fact]
        public void NonExistingPullController() =>
            AsyncContext.Run(
                async () =>
                {
                    var context = this.ContextFactory.CreateContext();

                    var exceptionThrown = false;
                    try
                    {
                        await context.Load(new { step = 0 }, "ThisIsWrong");
                    }
                    catch (HttpRequestException)
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);
                });
    }
}
