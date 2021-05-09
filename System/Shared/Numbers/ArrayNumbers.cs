// <copyright file="DefaultOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Xml;

    public class ArrayNumbers : INumbers
    {
        public object? From(IEnumerable<long> values)
        {
            switch (values)
            {
                case null:
                case long[] {Length: 0}:
                    return null;
                case long[] {Length: 1} array:
                    return array[0];
                case long[] array:
                    return array;
                case ICollection<long> collection:
                    return collection;
                default:
                    using (var enumerator = values.GetEnumerator())
                    {
                        if (!enumerator.MoveNext())
                        {
                            return null;
                        }

                        var first = enumerator.Current;
                        if (!enumerator.MoveNext())
                        {
                            return first;
                        }

                        var second = enumerator.Current;
                        if (!enumerator.MoveNext())
                        {
                            return first < second ? new[] {first, second} : new[] {second, first};
                        }

                        var list = new List<long> {first, second};

                        while (enumerator.MoveNext())
                        {
                            list.Add(enumerator.Current);
                        }

                        return list.ToArray();
                    }
            }
        }

        public object? From(ICollection<long> values)
        {
            var count = values.Count;

            switch (count)
            {
                case 0:
                    return null;
                case 1:
                    return values.First();
                default:
                    var array = new long[count];
                    values.CopyTo(array, 0);
                    return this.From(array);
            }
        }

        public object? From(params long[] values) =>
            values.Length switch
            {
                0 => null,
                1 => values[0],
                _ => values
            };

        public object? From(long value) => value;

        public object? Add(object? numbers, long other)
        {
            switch (numbers)
            {
                case null:
                    return other;
                case long value when value == other:
                    return value;
                case long value:
                    return value < other ? new[] {value, other} : new[] {other, value};
                default:
                {
                    var array = (long[])numbers;
                    var index = Array.BinarySearch(array, other);

                    if (index >= 0)
                    {
                        return array;
                    }

                    index = ~index;

                    var result = new long[array.Length + 1];
                    result[index] = other;

                    if (index == 0)
                    {
                        Array.Copy(array, 0, result, 1, array.Length);
                    }
                    else if (index == array.Length)
                    {
                        Array.Copy(array, result, array.Length);
                    }
                    else
                    {
                        Array.Copy(array, result, index);
                        Array.Copy(array, index, result, index + 1, array.Length - index);
                    }

                    return result;
                }
            }
        }

        public object? Union(object? numbers, object? other)
        {
            switch (numbers)
            {
                case null:
                    return other;
                case long value:
                    return other switch
                    {
                        null => value,
                        long otherValue when value == otherValue => value,
                        long otherValue => value < otherValue ? new[] {value, otherValue} : new[] {otherValue, value},
                        _ => this.Add(other, value)
                    };

                default:
                {
                    var array = (long[])numbers;

                    switch (other)
                    {
                        case null:
                            return array;
                        case long otherValue:
                            return this.Add(array, otherValue);
                        default:
                        {
                            var otherArray = (long[])other;
                            var arrayLength = array.Length;
                            var otherArrayLength = otherArray.Length;

                            var result = new long[arrayLength + otherArrayLength];

                            var i = 0;
                            var j = 0;
                            var k = 0;

                            long? previous = null;
                            while (i < arrayLength && j < otherArrayLength)
                            {
                                var value = array[i];
                                var otherValue = otherArray[j];

                                if (value < otherValue)
                                {
                                    if (value != previous)
                                    {
                                        result[k++] = value;
                                    }

                                    i++;
                                    previous = value;
                                }
                                else
                                {
                                    if (otherValue != previous)
                                    {
                                        result[k++] = otherValue;
                                    }

                                    if (otherValue == value)
                                    {
                                        i++;
                                    }

                                    j++;
                                    previous = otherValue;
                                }
                            }

                            if (i < arrayLength)
                            {
                                var rest = arrayLength - i;
                                Array.Copy(array, i, result, k, rest);
                                k += rest;
                            }
                            else if (j < otherArrayLength)
                            {
                                var rest = otherArrayLength - j;
                                Array.Copy(otherArray, j, result, k, rest);
                                k += rest;
                            }

                            if (k < result.Length)
                            {
                                Array.Resize(ref result, k);
                            }

                            return result;
                        }
                    }
                }
            }
        }

        public object? Remove(object? numbers, long other)
        {
            switch (numbers)
            {
                case null:
                    return null;
                case long value when value == other:
                    return null;
                case long value:
                    return value;
                default:
                {
                    var array = (long[])numbers;
                    var index = Array.BinarySearch(array, other);

                    if (index < 0)
                    {
                        return array;
                    }

                    var result = new long[array.Length - 1];

                    if (index == 0)
                    {
                        Array.Copy(array, 1, result, 0, array.Length - 1);
                    }
                    else if (index == array.Length)
                    {
                        Array.Copy(array, result, array.Length - 1);
                    }
                    else
                    {
                        Array.Copy(array, result, index);
                        Array.Copy(array, index + 1, result, index, array.Length - index - 1);
                    }

                    return result;
                }
            }
        }

        public object? Except(object? numbers, object? other)
        {
            switch (numbers)
            {
                case null:
                    return null;
                case long value:
                    return other switch
                    {
                        null => value,
                        long otherValue when value != otherValue => value,
                        _ => null
                    };

                default:
                {
                    var array = (long[])numbers;

                    switch (other)
                    {
                        case null:
                            return array;
                        case long otherValue:
                            return this.Remove(array, otherValue);
                        default:
                        {
                            var otherArray = (long[])other;

                            var arrayLength = array.Length;
                            var otherArrayLength = otherArray.Length;

                            var result = new long[arrayLength];
                            var i = 0;
                            var j = 0;
                            var k = 0;

                            while (i < arrayLength && j<otherArrayLength)
                            {
                                var value = array[i];
                                var otherValue = otherArray[j];

                                if (value < otherValue)
                                {
                                    result[k++] = value;
                                    i++;
                                }
                                else if (value > otherValue)
                                {
                                    j++;
                                }
                                else
                                {
                                    i++;
                                }
                            }

                            if (i < arrayLength)
                            {
                                var rest = arrayLength - i;
                                Array.Copy(array, i, result, k, rest);
                                k += rest;
                            }

                            if (k < result.Length)
                            {
                                Array.Resize(ref result, k);
                            }

                            return result.Length != 0 ? result : null;
                        }
                    }
                }
            }
        }

        public IEnumerable<long> Enumerate(object? numbers) =>
            numbers switch
            {
                null => EmptyEnumerable.Instance,
                long value => GetEnumerator(value),
                _ => (IEnumerable<long>)numbers
            };

        private static IEnumerable<long> GetEnumerator(long value)
        {
            yield return value;
        }

        #region Empty

        private class EmptyEnumerable : IEnumerable<long>
        {
            public static readonly IEnumerable<long> Instance = new EmptyEnumerable();

            public IEnumerator<long> GetEnumerator() => EmptyEnumerator.Instance;

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        private class EmptyEnumerator : IEnumerator<long>
        {
            public static readonly EmptyEnumerator Instance = new EmptyEnumerator();

            public bool MoveNext() => false;

            public void Reset() { }

            public long Current => 0;

            object IEnumerator.Current => this.Current;

            public void Dispose() { }
        }

        #endregion
    }
}
