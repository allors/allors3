// <copyright file="PullTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Xunit;
    using PullResult = Allors.Workspace.Adapters.Remote.PullResult;
    using UnitConvert = Allors.Protocol.Json.SystemTextJson.UnitConvert;

    public class PullTests : Workspace.PullTests, IClassFixture<Fixture>
    {
        public PullTests(Fixture fixture) : base(fixture) => this.Profile = new Profile();

        public override IProfile Profile { get; }

        [Fact]
        public void ValuesSurviveSerialization()
        {
            var response = new PullResponse
            {
                v = new Dictionary<string, object>
                {
                    ["allorsBinary"] = new byte[] { 1, 2, 3 },
                    ["allorsBoolean"] = true,
                    ["allorsDateTime"] = new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc),
                    ["allorsDecimal"] = 12.34m,
                    ["allorsDouble"] = 123d,
                    ["allorsInteger"] = 1000,
                    ["allorsString"] = "a string",
                    ["allorsUnique"] = new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF"),
                },
            };
            var json = JsonSerializer.Serialize(response);
            var deserialized = JsonSerializer.Deserialize<PullResponse>(json);
            var session = (Allors.Workspace.Adapters.Session)this.Workspace.CreateSession();
            var result = new PullResult(session, deserialized, new UnitConvert());

            Assert.False(result.HasErrors);
            Assert.Equal(new byte[] { 1, 2, 3 }, result.GetValue<byte[]>("allorsBinary"));
            Assert.True(result.GetValue<bool>("allorsBoolean"));
            Assert.Equal(new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc), result.GetValue<DateTime>("allorsDateTime"));
            Assert.Equal(12.34m, result.GetValue<decimal>("allorsDecimal"));
            Assert.Equal(123d, result.GetValue<double>("allorsDouble"));
            Assert.Equal(1000, result.GetValue<int>("allorsInteger"));
            Assert.Equal("a string", result.GetValue<string>("allorsString"));
            Assert.Equal(new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF"), result.GetValue<Guid>("allorsUnique"));
        }
    }
}
