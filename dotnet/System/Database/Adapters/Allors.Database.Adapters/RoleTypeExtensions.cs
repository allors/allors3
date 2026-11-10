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
        /// Normalizes a unit that is set as a role, the same way on every adapter: the unit is normalized, and a
        /// decimal is rounded to the scale of the role type and refused when it is then too large for its precision.
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
        /// The unit is not compatible with the role type, or the rounded decimal is too large for its precision.
        /// </exception>
        public static object NormalizeRole(this IRoleType roleType, object unit)
        {
            var role = roleType.RoundToScale(roleType.Normalize(unit));

            if (role is decimal @decimal && !roleType.Fits(@decimal))
            {
                throw new ArgumentException($"The decimal {@decimal.ToString(System.Globalization.CultureInfo.InvariantCulture)} is too large for the precision {roleType.Precision} and the scale {roleType.Scale} of {roleType}.");
            }

            return role;
        }

        /// <summary>
        /// Rounds a decimal to the scale of the role type, half away from zero, as SQL Server and PostgreSQL do when
        /// they store it.
        /// </summary>
        /// <param name="roleType">
        ///     The role type.
        /// </param>
        /// <param name="unit">
        ///     The normalized unit.
        /// </param>
        /// <returns>
        /// The rounded decimal, or the unit itself when it is not a decimal.
        /// </returns>
        /// <exception cref="OverflowException">The rounded value is outside the range of decimal.</exception>
        public static object RoundToScale(this IRoleType roleType, object unit)
        {
            const int MaximumScale = 28;

            if (unit is decimal @decimal && roleType.Scale is int scale)
            {
                if (scale >= 0 && scale <= MaximumScale)
                {
                    return decimal.Round(@decimal, scale, MidpointRounding.AwayFromZero);
                }

                if (scale < 0)
                {
                    if (scale < -MaximumScale)
                    {
                        // A step of 10^29 exceeds decimal's range, but its midpoint is representable.
                        if (scale == -29 && Math.Abs(@decimal) >= 5e28m)
                        {
                            throw new OverflowException($"Rounding to scale {scale} exceeds the range of decimal.");
                        }

                        return 0m;
                    }

                    var step = 1m;
                    for (var i = 0; i > scale; i--)
                    {
                        step *= 10m;
                    }

                    // Dividing before rounding can turn a value just below a midpoint into the midpoint.
                    var remainder = @decimal % step;
                    var rounded = @decimal - remainder;
                    if (Math.Abs(remainder) >= step / 2m)
                    {
                        rounded += @decimal < 0 ? -step : step;
                    }

                    return rounded;
                }
            }

            return unit;
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
    }
}
