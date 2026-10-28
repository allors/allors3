// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.DatabaseRelation.Remote
{
    using System;
    using Allors;
    using Allors.Protocol.Json.Newtonsoft;
    using Workspace.Remote;
    using Xunit;

    public class UnitTests : DatabaseRelation.UnitTests, IClassFixture<Fixture>
    {
        public UnitTests(Fixture fixture) : base(fixture) => this.Profile = new Profile();

        public override IProfile Profile { get; }

        public static TheoryData<string, object, string> UnsupportedUnitConversions => new()
        {
            { UnitTags.Binary, 0, "Cannot convert System.Int32 to Binary (tag '1')." },
            { UnitTags.Boolean, 0L, "Cannot convert System.Int64 to Boolean (tag '2')." },
            { UnitTags.DateTime, 0f, "Cannot convert System.Single to DateTime (tag '3')." },
            { UnitTags.Decimal, true, "Cannot convert System.Boolean to Decimal (tag '4')." },
            { UnitTags.Float, new DateTime(2026, 10, 9, 12, 34, 56, DateTimeKind.Utc), "Cannot convert System.DateTime to Float (tag '5')." },
            { UnitTags.Integer, 0d, "Cannot convert System.Double to Integer (tag '6')." },
            { UnitTags.Integer, 1.5m, "Cannot convert System.Decimal to Integer (tag '6')." },
            { UnitTags.Integer, 2147483648L, "Cannot convert System.Int64 to Integer (tag '6'): 2147483648 is not a whole number from -2147483648 through 2147483647." },
            { UnitTags.Integer, -2147483649L, "Cannot convert System.Int64 to Integer (tag '6'): -2147483649 is not a whole number from -2147483648 through 2147483647." },
            { UnitTags.String, 0, "Cannot convert System.Int32 to String (tag '7')." },
            { UnitTags.Unique, 0, "Cannot convert System.Int32 to Unique (tag '8')." },
            { "99", "value", "Cannot convert System.String to unknown unit (tag '99')." },
        };

        [Theory]
        [MemberData(nameof(UnsupportedUnitConversions))]
        public void UnitConversionWithUnsupportedTag(string tag, object value, string message)
        {
            var convert = new UnitConvert();
            bool hasErrors;

            try
            {
                convert.UnitFromJson(tag, value);
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
        public void UnitConversionFromLongToInteger()
        {
            var convert = new UnitConvert();

            Assert.Equal(int.MinValue, Assert.IsType<int>(convert.UnitFromJson(UnitTags.Integer, (long)int.MinValue)));
            Assert.Equal(int.MaxValue, Assert.IsType<int>(convert.UnitFromJson(UnitTags.Integer, (long)int.MaxValue)));
        }
    }
}
