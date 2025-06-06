// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValuesTest.cs" company="Allors bv">
//   Copyright 2002-2012 Allors bv.
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Adapters
{
    using System;
    using System.Linq;
    using Meta;
    using Xunit;
    using DateTime = System.DateTime;

    public abstract class ValuesTest : Test
    {
        protected TestValues testValues = new TestValues();

        protected virtual int[] BinarySizes =>
            new[]
            {
                0,
                1,
                2,
                8000 - 1,
                8000, // SqlClient
                8000 + 1,
                2 ^ 16 - 1,
                2 ^ 16, // MySqlClient
                2 ^ 16 + 1,
                2 ^ 32 - 1,
                2 ^ 32, // MySqlClient
                2 ^ 32 + 1
            };

        protected virtual int[] StringSizes =>
            new[]
            {
                0,
                1,
                2,
                4000 - 1,
                4000, // SqlClient
                4000 + 1,
                8000 - 1,
                8000, // SqlClient
                8000 + 1,
                2 ^ 16 - 1,
                2 ^ 16, // MySqlClient
                2 ^ 16 + 1,
                2 ^ 32 - 1,
                2 ^ 32, // MySqlClient
                2 ^ 32 + 1
            };

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsBinary()
        {
            bool[] transactionFlags = { false, true };

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int binarySizeIndex = 0; binarySizeIndex < this.BinarySizes.Count(); binarySizeIndex++)
                {
                    int binarySize = this.BinarySizes[binarySizeIndex];

                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetBinaryRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            byte[] value = this.ValueGenerator.GenerateBinary(binarySize);

                            if (binarySize < testRoleType.RoleType.Size)
                            {
                                allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                                if (transactionFlag)
                                {
                                    this.GetTransaction().Commit();
                                }

                                Assert.Equal(value, allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                                Assert.Equal(value, allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsBoolean()
        {
            bool[] transactionFlags = { false, true };
            bool[] values = this.testValues.Booleans;

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    bool value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetBooleanRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.Equal(value, (bool)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (bool)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Initial empty
                for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                {
                    var testType = this.GetTestTypes()[testTypeIndex];
                    var allorsObject = this.GetTransaction().Create(testType);
                    if (transactionFlag)
                    {
                        this.GetTransaction().Commit();
                    }

                    var testRoleTypes = this.GetBooleanRoles(testType);
                    for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                    {
                        var testRoleType = testRoleTypes[testRoleTypeIndex];

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                    }
                }

                // Remove
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    bool value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetBooleanRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    bool value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        this.GetTransaction().Commit();

                        var testRoleTypes = this.GetBooleanRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Rollback();

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Set Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    bool value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetBooleanRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            bool value2 = !value;

                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value2);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (bool)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (bool)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Remove Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    bool value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetBooleanRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (bool)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (bool)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsDateTime()
        {
            bool[] transactionFlags = { false, true };
            var values = new DateTime[this.testValues.DateTimes.Count()];
            for (int i = 0; i < values.Count(); i++)
            {
                values[i] = this.testValues.DateTimes[i];
            }

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    var value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetDateTimeRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            var dateTime = (DateTime)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType);
                            if (!dateTime.Equals(value))
                            {
                                Console.WriteLine(dateTime);
                            }

                            Assert.InRange((DateTime)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType), value.AddMilliseconds(-1), value.AddMilliseconds(1));
                            Assert.InRange((DateTime)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType), value.AddMilliseconds(-1), value.AddMilliseconds(1));
                        }
                    }
                }

                // Initial empty
                for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                {
                    var testType = this.GetTestTypes()[testTypeIndex];
                    var allorsObject = this.GetTransaction().Create(testType);
                    if (transactionFlag)
                    {
                        this.GetTransaction().Commit();
                    }

                    var testRoleTypes = this.GetDateTimeRoles(testType);
                    for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                    {
                        var testRoleType = testRoleTypes[testRoleTypeIndex];

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                    }
                }

                // Remove
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    object value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetDateTimeRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    object value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        this.GetTransaction().Commit();

                        var testRoleTypes = this.GetDateTimeRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Rollback();

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Set Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    var value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetDateTimeRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            var value2 = this.ValueGenerator.GenerateDateTime();

                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value2);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.InRange((DateTime)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType), value.AddMilliseconds(-1), value.AddMilliseconds(1));
                            Assert.InRange((DateTime)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType), value.AddMilliseconds(-1), value.AddMilliseconds(1));
                        }
                    }
                }

                // Commit Remove Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    var value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetDateTimeRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.InRange((DateTime)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType), value.AddMilliseconds(-1), value.AddMilliseconds(1));
                            Assert.InRange((DateTime)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType), value.AddMilliseconds(-1), value.AddMilliseconds(1));
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsDecimal()
        {
            bool[] transactionFlags = { false, true };
            var values = new object[this.testValues.Decimals.Count()];
            for (int i = 0; i < values.Count(); i++)
            {
                values[i] = this.testValues.Decimals[i];
            }

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    object value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetDecimalRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.Equal(value, (decimal)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (decimal)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Initial empty
                for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                {
                    var testType = this.GetTestTypes()[testTypeIndex];
                    var allorsObject = this.GetTransaction().Create(testType);
                    if (transactionFlag)
                    {
                        this.GetTransaction().Commit();
                    }

                    var testRoleTypes = this.GetDecimalRoles(testType);
                    for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                    {
                        var testRoleType = testRoleTypes[testRoleTypeIndex];

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                    }
                }

                // Remove
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    object value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetDecimalRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    object value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        this.GetTransaction().Commit();

                        var testRoleTypes = this.GetDecimalRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Rollback();

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Set Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    object value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetDecimalRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            object value2 = this.ValueGenerator.GenerateDecimal();

                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value2);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (decimal)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (decimal)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Remove Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    object value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetDecimalRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (decimal)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (decimal)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsDouble()
        {
            bool[] transactionFlags = { false, true };
            double[] values = this.testValues.Floats;

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    double value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetFloatRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.Equal(value, (double)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (double)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Initial empty
                for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                {
                    var testType = this.GetTestTypes()[testTypeIndex];
                    var allorsObject = this.GetTransaction().Create(testType);
                    if (transactionFlag)
                    {
                        this.GetTransaction().Commit();
                    }

                    var testRoleTypes = this.GetFloatRoles(testType);
                    for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                    {
                        var testRoleType = testRoleTypes[testRoleTypeIndex];

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                    }
                }

                // Remove
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    double value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetFloatRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    double value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        this.GetTransaction().Commit();

                        var testRoleTypes = this.GetFloatRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Rollback();

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Set Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    double value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetFloatRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            double value2 = this.ValueGenerator.GenerateFloat();

                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value2);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (double)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (double)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Remove Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    double value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetFloatRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (double)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (double)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsInteger()
        {
            bool[] transactionFlags = { false, true };
            int[] values = this.testValues.Integers;

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    int value = values[valueIndex];

                    var testTypes = this.GetTestTypes();
                    for (int testTypeIndex = 0; testTypeIndex < testTypes.Count(); testTypeIndex++)
                    {
                        var testType = testTypes[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetIntegerRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.Equal(value, (int)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (int)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Initial empty
                for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                {
                    var testType = this.GetTestTypes()[testTypeIndex];
                    var allorsObject = this.GetTransaction().Create(testType);
                    if (transactionFlag)
                    {
                        this.GetTransaction().Commit();
                    }

                    var testRoleTypes = this.GetIntegerRoles(testType);
                    for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                    {
                        var testRoleType = testRoleTypes[testRoleTypeIndex];

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                    }
                }

                // Remove
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    int value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetIntegerRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    int value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        this.GetTransaction().Commit();

                        var testRoleTypes = this.GetIntegerRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Rollback();

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Set Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    int value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetIntegerRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            int value2 = this.ValueGenerator.GenerateInteger();

                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value2);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (int)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (int)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Remove Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    int value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetIntegerRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (int)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (int)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsString()
        {
            bool[] transactionFlags = { false, true };

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int stringSizeIndex = 0; stringSizeIndex < this.StringSizes.Count(); stringSizeIndex++)
                {
                    int stringSize = this.StringSizes[stringSizeIndex];

                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetStringRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            string value = this.ValueGenerator.GenerateString(stringSize);

                            if (stringSize < testRoleType.RoleType.Size)
                            {
                                allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                                if (transactionFlag)
                                {
                                    this.GetTransaction().Commit();
                                }

                                Assert.Equal(value, allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                                Assert.Equal(value, allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AllorsUnique()
        {
            bool[] transactionFlags = { false, true };
            Guid[] values = this.testValues.Uniques;

            for (int transactionFlagIndex = 0; transactionFlagIndex < transactionFlags.Count(); transactionFlagIndex++)
            {
                var transactionFlag = transactionFlags[transactionFlagIndex];

                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    Guid value = values[valueIndex];

                    var testTypes = this.GetTestTypes();
                    for (int testTypeIndex = 0; testTypeIndex < testTypes.Count(); testTypeIndex++)
                    {
                        var testType = testTypes[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        var testRoleTypes = this.GetUniqueRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.Equal(value, (Guid)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (Guid)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Initial empty
                for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                {
                    var testType = this.GetTestTypes()[testTypeIndex];
                    var allorsObject = this.GetTransaction().Create(testType);
                    if (transactionFlag)
                    {
                        this.GetTransaction().Commit();
                    }

                    var testRoleTypes = this.GetUniqueRoles(testType);
                    for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                    {
                        var testRoleType = testRoleTypes[testRoleTypeIndex];

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                        if (transactionFlag)
                        {
                            this.GetTransaction().Commit();
                        }

                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                    }
                }

                // Remove
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    Guid value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetUniqueRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);
                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            if (transactionFlag)
                            {
                                this.GetTransaction().Commit();
                            }

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                // Set
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    Guid value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);
                        this.GetTransaction().Commit();

                        var testRoleTypes = this.GetUniqueRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Rollback();

                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.False(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Set Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    Guid value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetUniqueRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            Guid value2 = this.ValueGenerator.GenerateUnique();

                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value2);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (Guid)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (Guid)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }

                // Commit Remove Rollback
                for (int valueIndex = 0; valueIndex < values.Count(); valueIndex++)
                {
                    Guid value = values[valueIndex];
                    for (int testTypeIndex = 0; testTypeIndex < this.GetTestTypes().Length; testTypeIndex++)
                    {
                        var testType = this.GetTestTypes()[testTypeIndex];
                        var allorsObject = this.GetTransaction().Create(testType);

                        var testRoleTypes = this.GetUniqueRoles(testType);
                        for (int testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                        {
                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                            allorsObject.Strategy.SetUnitRole(testRoleType.RoleType, value);

                            this.GetTransaction().Commit();

                            allorsObject.Strategy.RemoveRole(testRoleType.RoleType);

                            this.GetTransaction().Rollback();

                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.True(allorsObject.Strategy.ExistRole(testRoleType.RoleType));
                            Assert.Equal(value, (Guid)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                            Assert.Equal(value, (Guid)allorsObject.Strategy.GetUnitRole(testRoleType.RoleType));
                        }
                    }
                }
            }
        }

        private IClass[] GetTestTypes() => this.GetMetaPopulation().Classes.ToArray();
    }
}
