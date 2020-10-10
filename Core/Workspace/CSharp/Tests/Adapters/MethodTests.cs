// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Adapters
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Nito.AsyncEx;
    using Xunit;

    public class MethodTests : Test
    {
        [Fact]
        public void Call() =>
            AsyncContext.Run(
                async () =>
                {
                    var context = this.Workspace.CreateSession();

                    var pull = new[]
                    {
                        new Pull
                        {
                            Extent = new Extent(M.Organisation.ObjectType),
                        },
                    };

                    var organisation = (await context.Load(pull)).GetCollection<Organisation>().First();

                    Assert.False(organisation.JustDidIt);

                    var result = await context.Call(organisation.JustDoIt);

                    Assert.False(result.HasErrors);

                    pull = new[]
                    {
                        new Pull
                        {
                            Object = organisation,
                        },
                    };

                    organisation = (await context.Load(pull)).GetObject<Organisation>();

                    context.Reset();

                    Assert.True(organisation.JustDidIt);
                });

    }
}
