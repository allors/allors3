// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{

    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Data;
    using Allors.Database.Protocol.Json;
    using Xunit;
    using Extent = Allors.Database.Data.Extent;

    public class PullInstantiateTests : ApiTest, IClassFixture<Fixture>
    {
        public PullInstantiateTests(Fixture fixture) : base(fixture) { }

        // TODO: Koen
    }
}
