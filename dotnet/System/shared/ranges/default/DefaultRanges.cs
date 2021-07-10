// <copyright file="DefaultOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DefaultRanges : IRanges
    {
        public IRange Load(IEnumerable<long>? sortedItems)
        {
            switch (sortedItems)
            {
                case null:
                case long[] { Length: 0 }:
                    return EmptyRange.Instance;
                case long[] array:
                    return new ArrayRange(array);
                case ICollection<long> collection:
                    var newArray = new long[collection.Count];
                    collection.CopyTo(newArray, 0);
                    return new ArrayRange(newArray);
                default:
                    return new ArrayRange(sortedItems.ToArray());
            }
        }

        public IRange Load(long item) => new ArrayRange(new[] { item });

        public IRange Load(params long[] sortedItems) =>
            sortedItems switch
            {
                null => EmptyRange.Instance,
                { Length: 0 } => EmptyRange.Instance,
                _ => new ArrayRange(sortedItems)
            };

        public IRange Ensure(object? nullable) => nullable switch
        {
            null => EmptyRange.Instance,
            IRange range => range,
            _ => throw new NotSupportedException($"Ensure is not supported from {nullable.GetType()}")
        };

        public IRange FromUnsorted(IEnumerable<long>? unsortedItems)
        {
            switch (unsortedItems)
            {
                case null:
                case long[] { Length: 0 }:
                    return EmptyRange.Instance;
                case long[] array:
                    var sortedArray = (long[])array.Clone();
                    Array.Sort(sortedArray);
                    return new ArrayRange(sortedArray);
                case ICollection<long> collection:
                    var newSortedArray = new long[collection.Count];
                    collection.CopyTo(newSortedArray, 0);
                    Array.Sort(newSortedArray);
                    return new ArrayRange(newSortedArray);
                default:
                    return new ArrayRange(unsortedItems.OrderBy(v => v).ToArray());
            }
        }

        public IRange Add(IRange? range, long item)
        {
            switch (range)
            {
                case null:
                case EmptyRange _:
                    return this.Load(item);
                case ArrayRange arrayRange:
                    switch (arrayRange.Items)
                    {
                        case var items when items.Length == 1 && items[0] == item:
                            return range;
                        case var items when items.Length == 1:
                            return items[0] < item ? new ArrayRange(new[] { items[0], item }) : new ArrayRange(new[] { item, items[0] });
                        default:
                            var array = arrayRange.Items;
                            var index = Array.BinarySearch(array, item);

                            if (index >= 0)
                            {
                                return range;
                            }

                            index = ~index;

                            var result = new long[array.Length + 1];
                            result[index] = item;

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

                            return new ArrayRange(result);
                    }

                default:
                    throw new ArgumentOutOfRangeException($"Range type {range.GetType()}");
            }
        }

        public IRange Remove(IRange? range, long item)
        {
            switch (range)
            {
                case null:
                    return EmptyRange.Instance;
                case EmptyRange _:
                    return range;
                case ArrayRange arrayRange:
                    switch (arrayRange.Items)
                    {
                        case null:
                        case var items when items.Length == 1 && items[0] == item:
                            return EmptyRange.Instance;
                        case var items when items.Length == 1:
                            return range;
                        default:
                            var array = arrayRange.Items;
                            var index = Array.BinarySearch(array, item);

                            if (index < 0)
                            {
                                return range;
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

                            return new ArrayRange(result);
                    }

                default:
                    throw new ArgumentOutOfRangeException($"Range type {range.GetType()}");
            }
        }

        public IRange Union(IRange? range, IRange? other)
        {
            switch (range)
            {
                case null:
                case EmptyRange _:
                    return other ?? EmptyRange.Instance;
                case ArrayRange _ when other == null:
                    return range;
                case ArrayRange _ when other is EmptyRange:
                    return range;
                case ArrayRange arrayRange when other is ArrayRange otherArrayRange:
                    switch (arrayRange.Items)
                    {
                        case var items when items.Length == 1:
                            return otherArrayRange.Items switch
                            {
                                var otherItems when otherItems.Length == 1 && items[0] == otherItems[0] => range,
                                var otherItems when otherItems.Length == 1 => items[0] < otherItems[0] ? new ArrayRange(new[] { items[0], otherItems[0] }) : new ArrayRange(new[] { otherItems[0], items[0] }),
                                _ => this.Add(other, items[0])
                            };

                        default:
                        {
                            var items = arrayRange.Items;

                            switch (otherArrayRange.Items)
                            {
                                case var otherItems when otherItems.Length == 1:
                                    return this.Add(range, otherItems[0]);
                                default:
                                    var otherArray = otherArrayRange.Items;
                                    var arrayLength = items.Length;
                                    var otherArrayLength = otherArray.Length;

                                    var result = new long[arrayLength + otherArrayLength];

                                    var i = 0;
                                    var j = 0;
                                    var k = 0;

                                    long? previous = null;
                                    while (i < arrayLength && j < otherArrayLength)
                                    {
                                        var value = items[i];
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
                                        Array.Copy(items, i, result, k, rest);
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

                                    return new ArrayRange(result);
                            }
                        }
                    }

                default:
                    throw new ArgumentOutOfRangeException($"Range type {range.GetType()}");
            }
        }

        public IRange Except(IRange? range, IRange? other)
        {
            if (other == null || other is EmptyRange)
            {
                return range ?? EmptyRange.Instance;
            }

            switch (range)
            {
                case null:
                    return EmptyRange.Instance;
                case EmptyRange _:
                    return range;
                case ArrayRange arrayRange:
                {
                    var otherArrayRange = (ArrayRange)other;

                    switch (arrayRange.Items)
                    {
                        case var items when items.Length == 1:
                            return otherArrayRange.Items switch
                            {
                                var otherItems when otherItems.Length == 1 && items[0] == otherItems[0] => EmptyRange.Instance,
                                var otherItems when otherItems.Length == 1 => range,
                                _ => other.Contains(items[0]) ? EmptyRange.Instance : range,
                            };

                        default:
                        {
                            var items = arrayRange.Items;

                            switch (otherArrayRange.Items)
                            {
                                case var otherItems when otherItems.Length == 1:
                                    return this.Remove(range, otherItems[0]);
                                default:
                                {
                                    var otherItems = otherArrayRange.Items;

                                    var itemsLength = items.Length;
                                    var otherArrayLength = otherItems.Length;

                                    var result = new long[itemsLength];
                                    var i = 0;
                                    var j = 0;
                                    var k = 0;

                                    while (i < itemsLength && j < otherArrayLength)
                                    {
                                        var value = items[i];
                                        var otherValue = otherItems[j];

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

                                    if (i < itemsLength)
                                    {
                                        var rest = itemsLength - i;
                                        Array.Copy(items, i, result, k, rest);
                                        k += rest;
                                    }

                                    if (k < result.Length)
                                    {
                                        Array.Resize(ref result, k);
                                    }

                                    return result.Length != 0 ? (IRange)new ArrayRange(result) : EmptyRange.Instance;
                                }
                            }
                        }
                    }
                }

                default:
                    throw new ArgumentOutOfRangeException($"Range type {range.GetType()}");
            }
        }
    }
}
