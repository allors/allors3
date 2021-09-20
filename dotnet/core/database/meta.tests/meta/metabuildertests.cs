// <copyright file="FilterTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
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

    }
}
