// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public struct Range : IEquatable<Range>, IEnumerable<long>
    {
        public Range(long[]? items) => this.Items = items;

        public long[]? Items { get; }

        public bool Equals(Range other)
        {
            var items = this.Items;
            var otherItems = other.Items;

            if (ReferenceEquals(items, otherItems))
            {
                return true;
            }

            if (items == null || otherItems == null)
            {
                return false;
            }

            var itemsLength = items.Length;

            if (itemsLength != otherItems.Length)
            {
                return false;
            }

            for (var i = 0; i < itemsLength; i++)
            {
                if (items[i] != otherItems[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj) => obj is Range other && this.Equals(other);

        public override int GetHashCode() => this.Items?.GetHashCode() ?? 0;

        public static bool operator ==(Range left, Range right) => left.Equals(right);

        public static bool operator !=(Range left, Range right) => !left.Equals(right);

        public IEnumerator<long> GetEnumerator() => this.Items != null ? ((IEnumerable<long>)this.Items).GetEnumerator() : EmptyEnumerator<long>.Instance;

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public long[] ToArray() => this.Items ?? Array.Empty<long>();

        public bool Contains(long item) =>
            this.Items switch
            {
                null => false,
                var items when items.Length == 1 => items[0] == item,
                _ => Array.BinarySearch(this.Items, item) >= 0,
            };

        public override string ToString()
        {
            return base.ToString();
        }

        private class EmptyEnumerator<TEmpty> : IEnumerator<TEmpty>
        {
            public static readonly EmptyEnumerator<TEmpty> Instance = new EmptyEnumerator<TEmpty>();

            public bool MoveNext() => false;

            public void Reset() { }

            TEmpty IEnumerator<TEmpty>.Current => throw new NotSupportedException("Range has no elements.");
            public object Current => throw new NotSupportedException("Range has no elements.");

            public void Dispose() { }
        }
    }
}
