// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.DatabaseRelation.Local
{
    using System;
    using System.Text.Json;
    using Allors;
    using Allors.Protocol.Json.SystemTextJson;
    using Workspace.Local;
    using Xunit;

    public class UnitTests : DatabaseRelation.UnitTests, IClassFixture<Fixture>
    {
        public UnitTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        public override IProfile Profile { get; }

        public static TheoryData<string, object> ProtocolUnits => new()
        {
            { UnitTags.Binary, new byte[] { 0, 127, 255 } },
            { UnitTags.Boolean, true },
            { UnitTags.Boolean, false },
            { UnitTags.DateTime, new DateTime(2026, 10, 9, 12, 34, 56, DateTimeKind.Utc) },
            { UnitTags.Decimal, 123456789.0123456789m },
            { UnitTags.Float, -123.456d },
            { UnitTags.Integer, int.MinValue },
            { UnitTags.Integer, int.MaxValue },
            { UnitTags.String, "a string" },
            { UnitTags.Unique, new Guid("0208bb9b-e87b-4ced-8dec-516e6778cd66") },
            { UnitTags.String, null },
        };

        [Theory]
        [MemberData(nameof(ProtocolUnits))]
        public void UnitConversionWithoutSerialization(string tag, object value)
        {
            var convert = new UnitConvert();
            Assert.Equal(value, convert.UnitFromJson(tag, convert.ToJson(value)));
        }

        [Theory]
        [MemberData(nameof(ProtocolUnits))]
        public void UnitConversionWithSerialization(string tag, object value)
        {
            var convert = new UnitConvert();
            var json = JsonSerializer.SerializeToElement(convert.ToJson(value));
            Assert.Equal(value, convert.UnitFromJson(tag, json));
        }
    }
}
