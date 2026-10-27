// <copyright file="ServicesTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System.Text.Json;
    using Allors.Protocol.Json.Api.Sync;
    using Xunit;

    public class SecurityTests : Workspace.SecurityTests, IClassFixture<Fixture>
    {
        public SecurityTests(Fixture fixture) : base(fixture) => this.Profile = new Profile();

        public override IProfile Profile { get; }

        [Fact]
        public void SyncSecuritySurvivesSerialization()
        {
            var request = new SyncRequest
            {
                o = new long[] { 42 },
                g = new[] { new long[] { 11, 4 }, new long[] { 12, 5 } },
                r = new[] { new long[] { 21, 7 }, new long[] { 22, 8 } },
            };
            var requestJson = global::Newtonsoft.Json.JsonConvert.SerializeObject(request);
            var serverRequest = JsonSerializer.Deserialize<SyncRequest>(requestJson);

            Assert.Equal(request.o, serverRequest.o);
            Assert.Equal(request.g, serverRequest.g);
            Assert.Equal(request.r, serverRequest.r);

            const string responseJson =
                "{\"o\":[],\"g\":[{\"i\":11,\"v\":4},{\"i\":12,\"v\":5,\"p\":null},{\"i\":13,\"v\":6,\"p\":[]},{\"i\":14,\"v\":7,\"p\":[31]}]," +
                "\"r\":[{\"i\":21,\"v\":7},{\"i\":22,\"v\":8,\"p\":null},{\"i\":23,\"v\":9,\"p\":[]},{\"i\":24,\"v\":10,\"p\":[31]}]," +
                "\"p\":[{\"i\":31,\"c\":\"class\",\"t\":\"role\",\"o\":1}]}";
            var response = global::Newtonsoft.Json.JsonConvert.DeserializeObject<SyncResponse>(responseJson);

            Assert.Empty(response.o);
            Assert.Null(response.g[0].p);
            Assert.Null(response.g[1].p);
            Assert.Empty(response.g[2].p);
            Assert.Equal(new long[] { 31 }, response.g[3].p);
            Assert.Null(response.r[0].p);
            Assert.Null(response.r[1].p);
            Assert.Empty(response.r[2].p);
            Assert.Equal(new long[] { 31 }, response.r[3].p);
            Assert.Equal(11, response.g[0].i);
            Assert.Equal(4, response.g[0].v);
            Assert.Equal(21, response.r[0].i);
            Assert.Equal(7, response.r[0].v);
            var permission = Assert.Single(response.p);
            Assert.Equal(31, permission.i);
            Assert.Equal("class", permission.c);
            Assert.Equal("role", permission.t);
            Assert.Equal(1, permission.o);
        }
    }
}
