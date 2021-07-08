// <copyright file="DefaultOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class UncachedRanges : IRanges
    {
        public Range New(IEnumerable<long>? sortedItems)
        {
            switch (sortedItems)
            {
                case null:
                case long[] { Length: 0 }:
                    return default;
                case long[] array:
                    return new Range(array);
                case ICollection<long> collection:
                    var newArray = new long[collection.Count];
                    collection.CopyTo(newArray, 0);
                    return new Range(newArray);
                default:
                    return new Range(sortedItems.ToArray());
            }
        }

        public Range New(params long[] sortedItems) =>
            sortedItems switch
            {
                { Length: 0 } => default,
                _ => new Range(sortedItems)
            };

        public Range New(long item) => new Range(new[] { item });

        public Range Import(IEnumerable<long>? unsortedItems)
        {
            switch (unsortedItems)
            {
                case null:
                case long[] { Length: 0 }:
                    return default;
                case long[] array:
                    var sortedArray = (long[])array.Clone();
                    Array.Sort(sortedArray);
                    return new Range(sortedArray);
                case ICollection<long> collection:
                    var newSortedArray = new long[collection.Count];
                    collection.CopyTo(newSortedArray, 0);
                    Array.Sort(newSortedArray);
                    return new Range(newSortedArray);
                default:
                    return new Range(unsortedItems.OrderBy(v => v).ToArray());
            }
        }

        public Range Import(params long[] unsortedItems) => this.Import((IEnumerable<long>)unsortedItems);

        public Range Add(Range range, long item)
        {
            switch (range.Items)
            {
                case null:
                    return new Range(new[] { item });
                case var items when items.Length == 1 && items[0] == item:
                    return range;
                case var items when items.Length == 1:
                    return items[0] < item ? new Range(new[] { items[0], item }) : new Range(new[] { item, items[0] });
                default:
                    var array = range.Items;
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

                    return new Range(result);
            }
        }

        public Range Remove(Range range, long item)
        {
            switch (range.Items)
            {
                case null:
                case var items when items.Length == 1 && items[0] == item:
                    return new Range(null);
                case var items when items.Length == 1:
                    return range;
                default:
                    var array = range.Items;
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

                    return new Range(result);
            }
        }

        public Range Union(Range range, Range other)
        {
            switch (range.Items)
            {
                case null:
                    return other;
                case var items when items.Length == 1:
                    return other.Items switch
                    {
                        null => range,
                        var otherItems when otherItems.Length == 1 && items[0] == otherItems[0] => range,
                        var otherItems when otherItems.Length == 1 => items[0] < otherItems[0] ? new Range(new[] { items[0], otherItems[0] }) : new Range(new[] { otherItems[0], items[0] }),
                        _ => this.Add(other, items[0])
                    };

                default:
                {
                    var items = range.Items;

                    switch (other.Items)
                    {
                        case null:
                            return range;
                        case var otherItems when otherItems.Length == 1:
                            return this.Add(range, otherItems[0]);
                        default:
                            var otherArray = other.Items;
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

                            return new Range(result);
                    }
                }
            }
        }

        public Range Except(Range range, Range other)
        {
            switch (range.Items)
            {
                case null:
                    return range;
                case var items when items.Length == 1:
                    return other.Items switch
                    {
                        null => range,
                        var otherItems when otherItems.Length == 1 && items[0] == otherItems[0] => default,
                        var otherItems when otherItems.Length == 1 => range,
                        _ => other.Contains(items[0]) ? default : range,
                    };

                default:
                {
                    var items = range.Items;

                    switch (other.Items)
                    {
                        case null:
                            return range;
                        case var otherItems when otherItems.Length == 0:
                            return this.Remove(range, otherItems[0]);
                        default:
                        {
                            var otherItems = other.Items;

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

                            return result.Length != 0 ? new Range(result) : default;
                        }
                    }
                }
            }
        }
    }
}
