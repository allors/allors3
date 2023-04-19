// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Shared.Ranges
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Collections;

    public struct RefRange<T> : IEquatable<RefRange<T>>, IEnumerable<T>
        where T : class, IComparable<T>
    {
        public static readonly RefRange<T> Empty = new RefRange<T>((T[]?)null);

        private readonly T[]? items;

        private RefRange(T item)
            : this(new[] { item })
        {
        }

        private RefRange(T[]? items) => this.items = items;

        public bool IsEmpty => this.items == null;

        public override string ToString() => this.items != null ? "[" + string.Join(", ", this.items.ToString()) + "]" : "[]";

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            if (this.items == null)
            {
                return EmptyEnumerator<T>.Instance;
            }

            return ((IEnumerable<T>)this.items).GetEnumerator();
        }

        public bool Contains(T item) => this.items != null && Array.BinarySearch(this.items, item) >= 0;

        public static RefRange<T> Ensure(object range) =>
            range switch
            {
                null => Empty,
                _ => (RefRange<T>)range
            };

        public static RefRange<T> Load(IEnumerable<T>? sortedItems)
        {
            switch (sortedItems)
            {
                case null:
                case T[] { Length: 0 }:
                    return Empty;
                case T[] array:
                    return new RefRange<T>(array);
                case ICollection<T> collection:
                    var newArray = new T[collection.Count];
                    collection.CopyTo(newArray, 0);
                    return new RefRange<T>(newArray);
                default:
                    var materialized = sortedItems.ToArray();
                    if (materialized.Length == 0)
                    {
                        return Empty;
                    }

                    return new RefRange<T>(materialized);
            }
        }

        public static RefRange<T> Load(T item) => new RefRange<T>(new[] { item });

        public static RefRange<T> Load(params T[] sortedItems) =>
            sortedItems switch
            {
                null => Empty,
                { Length: 0 } => Empty,
                _ => new RefRange<T>(sortedItems),
            };

        public static RefRange<T> Import(IEnumerable<T>? unsortedItems)
        {
            switch (unsortedItems)
            {
                case null:
                case T[] { Length: 0 }:
                    return Empty;
                case T[] array:
                    var sortedArray = (T[])array.Clone();
                    Array.Sort(sortedArray);
                    return new RefRange<T>(sortedArray);
                case ICollection<T> collection:
                    var newSortedArray = new T[collection.Count];
                    collection.CopyTo(newSortedArray, 0);
                    Array.Sort(newSortedArray);
                    return new RefRange<T>(newSortedArray);
                default:
                    var materialized = unsortedItems.ToArray();
                    if (materialized.Length == 0)
                    {
                        return Empty;
                    }

                    Array.Sort(materialized);
                    return new RefRange<T>(materialized);
            }
        }

        public T[]? Save() => this.items;

        public RefRange<T> Add(T item)
        {
            switch (this.items)
            {
                case null:
                    return new RefRange<T>(item);
                case var v when v.Length == 1 && v[0].CompareTo(item) == 0:
                    return this;
                case var v when v.Length == 1:
                    return v[0].CompareTo(item) < 0 ? new RefRange<T>(new[] { v[0], item }) : new RefRange<T>(new[] { item, v[0] });
                default:
                    var index = Array.BinarySearch(this.items, item);

                    if (index >= 0)
                    {
                        return this;
                    }

                    index = ~index;

                    var result = new T[this.items.Length + 1];
                    result[index] = item;

                    if (index == 0)
                    {
                        Array.Copy(this.items, 0, result, 1, this.items.Length);
                    }
                    else if (index == this.items.Length)
                    {
                        Array.Copy(this.items, result, this.items.Length);
                    }
                    else
                    {
                        Array.Copy(this.items, result, index);
                        Array.Copy(this.items, index, result, index + 1, this.items.Length - index);
                    }

                    return new RefRange<T>(result);
            }
        }

        public RefRange<T> Remove(T item)
        {
            switch (this.items)
            {
                case null:
                case var v when v.Length == 1 && v[0].CompareTo(item) == 0:
                    return Empty;
                case var v when v.Length == 1:
                    return this;
                default:
                    var array = this.items;
                    var index = Array.BinarySearch(array, item);

                    if (index < 0)
                    {
                        return this;
                    }

                    var result = new T[array.Length - 1];

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

                    return new RefRange<T>(result);
            }
        }

        public RefRange<T> Union(RefRange<T> other)
        {
            if (other.items == null)
            {
                return this;
            }

            if (this.items == null)
            {
                return other;
            }

            if (this.items.Length == 1)
            {
                return other.items switch
                {
                    var otherItems when otherItems.Length == 1 && this.items[0].CompareTo(otherItems[0]) == 0 => this,
                    var otherItems when otherItems.Length == 1 => this.items[0].CompareTo(otherItems[0]) < 0
                        ? new RefRange<T>(new[] { this.items[0], otherItems[0] })
                        : new RefRange<T>(new[] { otherItems[0], this.items[0] }),
                    _ => other.Add(this.items[0]),
                };
            }

            switch (other.items)
            {
                case var otherItems when otherItems.Length == 1:
                    return this.Add(otherItems[0]);
                default:
                    var otherArray = other.items;
                    var arrayLength = this.items.Length;
                    var otherArrayLength = otherArray.Length;

                    var result = new T[arrayLength + otherArrayLength];

                    var i = 0;
                    var j = 0;
                    var k = 0;

                    T? previous = null;
                    while (i < arrayLength && j < otherArrayLength)
                    {
                        var value = this.items[i];
                        var otherValue = otherArray[j];

                        if (value.CompareTo(otherValue) < 0)
                        {
                            if (previous == null || value.CompareTo(previous) != 0)
                            {
                                result[k++] = value;
                            }

                            i++;
                            previous = value;
                        }
                        else
                        {
                            if (previous == null || otherValue.CompareTo(previous) != 0)
                            {
                                result[k++] = otherValue;
                            }

                            if (otherValue.CompareTo(value) == 0)
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
                        Array.Copy(this.items, i, result, k, rest);
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

                    return new RefRange<T>(result);
            }
        }

        public RefRange<T> Except(RefRange<T> other)
        {
            if (other.items == null)
            {
                return this;
            }

            switch (this.items)
            {
                case null:
                    return Empty;

                case var v when v.Length == 1:
                    return other.items switch
                    {
                        var otherItems when otherItems.Length == 1 && v[0].CompareTo(otherItems[0]) == 0 => Empty,
                        var otherItems when otherItems.Length == 1 => this,
                        _ => other.Contains(v[0]) ? Empty : this,
                    };

                default:
                {
                    switch (other.items)
                    {
                        case var otherItems when otherItems.Length == 1:
                            return this.Remove(otherItems[0]);
                        default:
                        {
                            var otherItems = other.items;

                            var itemsLength = this.items.Length;
                            var otherArrayLength = otherItems.Length;

                            var result = new T[itemsLength];
                            var i = 0;
                            var j = 0;
                            var k = 0;

                            while (i < itemsLength && j < otherArrayLength)
                            {
                                var value = this.items[i];
                                var otherValue = otherItems[j];

                                if (value.CompareTo(otherValue) < 0)
                                {
                                    result[k++] = value;
                                    i++;
                                }
                                else if (value.CompareTo(otherValue) > 0)
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
                                Array.Copy(this.items, i, result, k, rest);
                                k += rest;
                            }

                            if (k < result.Length)
                            {
                                Array.Resize(ref result, k);
                            }

                            return result.Length != 0 ? new RefRange<T>(result) : Empty;
                        }
                    }
                }
            }
        }

        public bool Equals(RefRange<T> other)
        {
            if (this.items == null)
            {
                return other.items == null;
            }

            if (other.items == null)
            {
                return false;
            }

            if (ReferenceEquals(this.items, other.items))
            {
                return true;
            }

            if (this.items.Length != other.items.Length)
            {
                return false;
            }

            for (var i = 0; i < this.items.Length; i++)
            {
                if (this.items[i].CompareTo(other.items[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
