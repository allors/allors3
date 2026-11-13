// <copyright file="RoleTypeExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;

    using Meta;

    public static class RoleTypeExtensions
    {
        /// <summary>
        /// Assert that the unit is compatible with the IObjectType of the RoleType.
        /// </summary>
        /// <param name="roleType">
        ///     The role type.
        /// </param>
        /// <param name="unit">
        ///     The unit to normalize.
        /// </param>
        /// <returns>
        /// The normalized unit.
        /// </returns>
        public static object Normalize(this IRoleType roleType, object unit)
        {
            if (unit == null)
            {
                return null;
            }

            var unitType = (IUnit)roleType.ObjectType;
            var unitTypeTag = unitType.Tag;

            var normalizedUnit = unit;

            switch (unitTypeTag)
            {
                case UnitTags.String:
                    if (!(unit is string))
                    {
                        throw new ArgumentException("RoleType is not a String.");
                    }

                    var stringUnit = (string)unit;
                    var size = roleType.Size;
                    if (size > -1 && stringUnit.Length > size)
                    {
                        throw new ArgumentException("Size of relationType " + roleType + " is too big (" + stringUnit.Length + ">" + size + ").");
                    }

                    break;
                case UnitTags.Integer:
                    if (!(unit is int))
                    {
                        throw new ArgumentException("RoleType is not an Integer.");
                    }

                    break;
                case UnitTags.DateTime:
                    if (unit is DateTime)
                    {
                        var dateTime = (DateTime)normalizedUnit;
                        if (dateTime != DateTime.MinValue && dateTime != DateTime.MaxValue)
                        {
                            switch (dateTime.Kind)
                            {
                                case DateTimeKind.Local:
                                    dateTime = dateTime.ToUniversalTime();
                                    break;
                                case DateTimeKind.Unspecified:
                                    throw new ArgumentException("DateTime value is of DateTimeKind.Kind Unspecified. \nUnspecified is only allowed for DateTime.MaxValue and DateTime.MinValue, use DateTimeKind.Utc or DateTimeKind.Local instead.");
                            }

                            normalizedUnit = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, DateTimeKind.Utc);
                        }
                        else
                        {
                            // The minimum and the maximum keep their ticks and have the kind Utc, as every date has.
                            normalizedUnit = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        }
                    }
                    else if (!(unit is DateTime))
                    {
                        throw new ArgumentException("RoleType is not a DateTime.");
                    }

                    break;
                case UnitTags.Decimal:
                    if (unit is int || unit is long || unit is float || unit is double)
                    {
                        normalizedUnit = Convert.ToDecimal(unit);
                    }
                    else if (!(unit is decimal))
                    {
                        throw new ArgumentException("RoleType is not a Decimal.");
                    }

                    break;
                case UnitTags.Float:
                    if (unit is int || unit is long || unit is float)
                    {
                        normalizedUnit = Convert.ToDouble(unit);
                    }
                    else if (!(unit is double))
                    {
                        throw new ArgumentException("RoleType is not a Float.");
                    }

                    break;
                case UnitTags.Boolean:
                    if (!(unit is bool))
                    {
                        throw new ArgumentException("RoleType is not a Boolean.");
                    }

                    break;
                case UnitTags.Unique:
                    if (!(unit is Guid))
                    {
                        throw new ArgumentException("RoleType is not a Boolean.");
                    }

                    break;
                case UnitTags.Binary:
                    if (!(unit is byte[]))
                    {
                        throw new ArgumentException("RoleType is not a Boolean.");
                    }

                    break;
                default:
                    throw new ArgumentException("Unknown Unit IObjectType: " + unitTypeTag);
            }

            return normalizedUnit;
        }

        /// <summary>
        /// Normalizes a unit that is set as a role, the same way on every adapter: the unit is normalized, a decimal
        /// that does not fit the precision and the scale of the role type is refused and one that fits has the
        /// decimals of the scale, and a unit that is not a value on every adapter is refused. A decimal is not
        /// rounded: the rule that reports a decimal on a load refuses it here.
        /// </summary>
        /// <param name="roleType">
        ///     The role type.
        /// </param>
        /// <param name="unit">
        ///     The unit that is set.
        /// </param>
        /// <returns>
        /// The unit as the role holds it.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The unit is not compatible with the role type, the decimal has more digits than the precision or more
        /// decimals than the scale of the role type, or the unit is not a value on every adapter.
        /// </exception>
        public static object NormalizeRole(this IRoleType roleType, object unit)
        {
            var role = roleType.Normalize(unit);

            if (role is decimal @decimal)
            {
                if (!roleType.Fits(@decimal))
                {
                    throw new ArgumentException($"The decimal {@decimal.ToString(System.Globalization.CultureInfo.InvariantCulture)} has more digits than the precision {roleType.Precision} or more decimals than the scale {roleType.Scale} of {roleType}: a decimal is not rounded when a role is set.");
                }

                role = WithDecimalsOfScale(roleType, role, @decimal);
            }

            roleType.AssertValue(role);

            return role;
        }

        /// <summary>
        /// Refuses a unit that is not a value on every adapter: a float that is NaN or an infinity, a string with the
        /// character U+0000 or with a lone surrogate, and a binary that is longer than the size of the role type.
        /// </summary>
        /// <param name="roleType">
        ///     The role type.
        /// </param>
        /// <param name="unit">
        ///     The unit, of the type that the role type holds.
        /// </param>
        /// <exception cref="ArgumentException">The unit is not a value on every adapter.</exception>
        public static void AssertValue(this IRoleType roleType, object unit)
        {
            switch (unit)
            {
                case double @double when !double.IsFinite(@double):
                    throw new ArgumentException($"The float {@double.ToString(System.Globalization.CultureInfo.InvariantCulture)} is not a value of {roleType}: a float role holds a finite number.");

                case string @string:
                    AssertString(roleType, @string);
                    break;

                case byte[] binary when roleType.Size > -1 && binary.Length > roleType.Size:
                    throw new ArgumentException($"The binary of {binary.Length} bytes is longer than the size {roleType.Size} of {roleType}.");
            }
        }

        /// <summary>
        /// Whether the role type can hold a decimal as it is: the scale preserves its value and its magnitude is less
        /// than 10^(precision - scale).
        /// </summary>
        /// <param name="roleType">
        ///     The role type.
        /// </param>
        /// <param name="value">
        ///     The decimal.
        /// </param>
        /// <returns>
        /// True when the decimal fits.
        /// </returns>
        public static bool Fits(this IRoleType roleType, decimal value)
        {
            const int MaximumScale = 28;

            var precision = roleType.Precision;
            var scale = roleType.Scale;

            if (value == 0)
            {
                return true;
            }

            if (scale < 0)
            {
                if (scale < -MaximumScale)
                {
                    return false;
                }

                var step = 1m;
                for (var i = 0; i > scale; i--)
                {
                    step *= 10m;
                }

                if (value % step != 0)
                {
                    return false;
                }
            }
            else if (scale <= MaximumScale && decimal.Round(value, scale.Value) != value)
            {
                return false;
            }

            var digits = (long?)precision - (scale ?? 0);
            if (digits <= -MaximumScale)
            {
                return false;
            }

            if (digits <= MaximumScale)
            {
                var limit = 1m;
                for (var i = 0; i < digits; i++)
                {
                    limit *= 10m;
                }

                for (var i = 0; i > digits; i--)
                {
                    limit /= 10m;
                }

                return Math.Abs(value) < limit;
            }

            return true;
        }

        // Gives a decimal that fits its role type the decimals of the scale, as SQL Server and PostgreSQL return it:
        // 1.5 is 1.50 at the scale 2. A scale below zero leaves no decimals, and a zero has no sign. Where decimal
        // has no room for the zeros of the scale, the decimal keeps the decimals that it has, and a scale above 28,
        // the most decimals that decimal has, leaves the decimal as it is. A decimal that has its form already is
        // returned as the object that it is.
        private static object WithDecimalsOfScale(IRoleType roleType, object role, decimal value)
        {
            const int MaximumScale = 28;

            if (!(roleType.Scale is int scale) || scale > MaximumScale)
            {
                return role;
            }

            var decimals = Math.Max(scale, 0);

            if (value.Scale == decimals)
            {
                return decimal.IsNegative(value) && value == 0 ? new decimal(0, 0, 0, false, (byte)decimals) : role;
            }

            if (value == 0)
            {
                return new decimal(0, 0, 0, false, (byte)decimals);
            }

            if (value.Scale > decimals)
            {
                // A decimal that fits has only zeros beyond the scale, and those go.
                return decimal.Round(value, decimals);
            }

            // The sum has the decimals of the zero when decimal has room for them, and fewer when it has not.
            var sum = value + new decimal(0, 0, 0, false, (byte)decimals);
            return sum.Scale == decimals ? sum : role;
        }

        private static void AssertString(IRoleType roleType, string value)
        {
            const char FirstSurrogate = (char)0xD800;
            const char LastSurrogate = (char)0xDFFF;

            var index = value.IndexOf((char)0);
            if (index >= 0)
            {
                throw new ArgumentException($"The string for {roleType} holds the character U+0000 at index {index}, which is not part of a string value.");
            }

            // A lone surrogate is not part of a string value: a surrogate counts in a pair only, a high surrogate that
            // a low surrogate follows.
            var characters = value.AsSpan();
            index = 0;
            while (true)
            {
                var offset = characters.Slice(index).IndexOfAnyInRange(FirstSurrogate, LastSurrogate);
                if (offset < 0)
                {
                    return;
                }

                // Pairs often follow one another, so that the pairs of a run are checked without a new search.
                for (index += offset; index < characters.Length && char.IsSurrogate(characters[index]); index += 2)
                {
                    if (!char.IsHighSurrogate(characters[index]) || index + 1 == characters.Length || !char.IsLowSurrogate(characters[index + 1]))
                    {
                        throw LoneSurrogate(roleType, characters[index], index);
                    }
                }
            }
        }

        private static ArgumentException LoneSurrogate(IRoleType roleType, char surrogate, int index) =>
            new ArgumentException($"The string for {roleType} holds the lone surrogate U+{(int)surrogate:X4} at index {index}, which is not part of a string value.");
    }
}
