// <copyright file="ReadStoreValues.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Copies decoded values without retaining arbitrary mutable objects. Copies happen before acquiring the store lock.
    /// </summary>
    internal static class ReadStoreValues
    {
        private const long MaximumInteger = 9007199254740991;

        internal static long Id(long value, string parameter)
        {
            if (value <= 0 || value > MaximumInteger)
            {
                throw new ArgumentOutOfRangeException(parameter, "Use a positive database ID no greater than 9007199254740991.");
            }

            return value;
        }

        internal static long Version(long value, string parameter)
        {
            if (value < 0 || value > MaximumInteger)
            {
                throw new ArgumentOutOfRangeException(parameter, "Use a nonnegative version no greater than 9007199254740991.");
            }

            return value;
        }

        internal static string Required(string value, string parameter)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Supply a nonempty identifier or tag.", parameter);
            }

            return value;
        }

        internal static ReadSecurityKey[] Security(IEnumerable<ReadSecurityKey> values, string kind)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values), "Supply the complete security set, including an explicit empty set.");
            }

            var array = values.ToArray();
            if (array.Any(v => v == null || v.Kind != kind))
            {
                throw new ArgumentException("Supply only non-null " + kind + " keys in this provenance set.", nameof(values));
            }

            array = array.Distinct().OrderBy(v => v.Id).ThenBy(v => v.Version).ToArray();
            if (array.GroupBy(v => v.Id).Any(v => v.Count() > 1))
            {
                throw new ArgumentException("A record's complete provenance must contain only one version of each security ID.", nameof(values));
            }

            return array;
        }

        internal static object Copy(object value) => Copy(value, new List<object>(), 0);

        internal static bool Equal(object left, object right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            if (left is IDictionary<string, object> leftMap && right is IDictionary<string, object> rightMap)
            {
                return leftMap.Count == rightMap.Count && leftMap.All(pair => rightMap.TryGetValue(pair.Key, out var value) && Equal(pair.Value, value));
            }

            if (left is Array leftArray && right is Array rightArray)
            {
                return leftArray.GetType() == rightArray.GetType() && leftArray.Length == rightArray.Length &&
                    leftArray.Cast<object>().Zip(rightArray.Cast<object>(), Equal).All(v => v);
            }

            return left.GetType() == right.GetType() && left.Equals(right);
        }

        private static object Copy(object value, List<object> ancestors, int depth)
        {
            if (value == null || value is string || value is bool || value is char || value is byte || value is sbyte ||
                value is short || value is ushort || value is int || value is uint || value is long || value is ulong ||
                value is decimal || value is DateTime || value is DateTimeOffset || value is TimeSpan || value is Guid)
            {
                return value;
            }

            if (value is double number && !double.IsNaN(number) && !double.IsInfinity(number))
            {
                return value;
            }

            if (value is float single && !float.IsNaN(single) && !float.IsInfinity(single))
            {
                return value;
            }

            if (depth >= 64 || ancestors.Any(v => ReferenceEquals(v, value)))
            {
                throw new ArgumentException("Use an acyclic decoded value tree with fewer than 64 nested containers.", nameof(value));
            }

            ancestors.Add(value);
            try
            {
                if (value is IReadOnlyDictionary<string, object> readOnlyMap)
                {
                    return readOnlyMap.ToDictionary(pair => pair.Key, pair => Copy(pair.Value, ancestors, depth + 1), StringComparer.Ordinal);
                }

                if (value is IDictionary<string, object> map)
                {
                    return map.ToDictionary(pair => pair.Key, pair => Copy(pair.Value, ancestors, depth + 1), StringComparer.Ordinal);
                }

                if (value is Array array && array.Rank == 1 && array.GetLowerBound(0) == 0)
                {
                    var result = (Array)array.Clone();
                    for (var i = 0; i < result.Length; i++)
                    {
                        result.SetValue(Copy(array.GetValue(i), ancestors, depth + 1), i);
                    }

                    return result;
                }

                if (value is IList list)
                {
                    return list.Cast<object>().Select(v => Copy(v, ancestors, depth + 1)).ToArray();
                }
            }
            finally
            {
                ancestors.RemoveAt(ancestors.Count - 1);
            }

            throw new ArgumentException("Use decoded scalar, date, binary, array, or string-keyed dictionary values; unsupported mutable values cannot be retained.", nameof(value));
        }
    }
}
