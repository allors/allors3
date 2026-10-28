// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.DatabaseRelation.Remote
{
    using System;
    using System.Text.Json;
    using Allors;
    using Allors.Protocol.Json.SystemTextJson;
    using Workspace.Remote;
    using Xunit;

    public class UnitTests : DatabaseRelation.UnitTests, IClassFixture<Fixture>
    {
        public UnitTests(Fixture fixture) : base(fixture) => this.Profile = new Profile();

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

        public static TheoryData<string, object, string> UnsupportedUnitConversions => new()
        {
            { UnitTags.Binary, 0, "Cannot convert JSON Number to Binary (tag '1')." },
            { UnitTags.Boolean, 0, "Cannot convert JSON Number to Boolean (tag '2')." },
            { UnitTags.DateTime, 0, "Cannot convert JSON Number to DateTime (tag '3')." },
            { UnitTags.Decimal, true, "Cannot convert JSON Boolean to Decimal (tag '4')." },
            { UnitTags.Float, "0", "Cannot convert JSON String to Float (tag '5')." },
            { UnitTags.Integer, "0", "Cannot convert JSON String to Integer (tag '6')." },
            { UnitTags.Integer, 1.5, "Cannot convert JSON Number to Integer (tag '6'): 1.5 is not a whole number from -2147483648 through 2147483647." },
            { UnitTags.Integer, 2147483648L, "Cannot convert JSON Number to Integer (tag '6'): 2147483648 is not a whole number from -2147483648 through 2147483647." },
            { UnitTags.Integer, -2147483649L, "Cannot convert JSON Number to Integer (tag '6'): -2147483649 is not a whole number from -2147483648 through 2147483647." },
            { UnitTags.Integer, new[] { 0 }, "Cannot convert JSON Array to Integer (tag '6')." },
            { UnitTags.String, 0, "Cannot convert JSON Number to String (tag '7')." },
            { UnitTags.Unique, 0, "Cannot convert JSON Number to Unique (tag '8')." },
            { "99", "value", "Cannot convert JSON String to unknown unit (tag '99')." },
        };

        [Theory]
        [MemberData(nameof(UnsupportedUnitConversions))]
        public void UnitConversionWithUnsupportedElement(string tag, object value, string message)
        {
            var convert = new UnitConvert();
            var json = JsonSerializer.SerializeToElement(value);
            bool hasErrors;

            try
            {
                convert.UnitFromJson(tag, json);
                hasErrors = false;
            }
            catch (Exception exception)
            {
                hasErrors = true;
                Assert.Equal(typeof(Exception), exception.GetType());
                Assert.Equal(message, exception.Message);
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public void UnitConversionWithUnknownTag()
        {
            var convert = new UnitConvert();
            bool hasErrors;

            try
            {
                convert.UnitFromJson("99", "value");
                hasErrors = false;
            }
            catch (Exception exception)
            {
                hasErrors = true;
                Assert.Equal(typeof(Exception), exception.GetType());
                Assert.Equal("Cannot convert System.String to unknown unit (tag '99').", exception.Message);
            }

            Assert.True(hasErrors);
        }
    }
}
