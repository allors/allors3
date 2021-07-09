
// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class EmptyRange : IRange
    {
        private static readonly long[] EmptyArray = Array.Empty<long>();
        private static readonly EmptyEnumerator Enumerator = new EmptyEnumerator();

        public static readonly EmptyRange Instance = new EmptyRange();

        private EmptyRange() { }

        public bool Equals(IRange other) => ReferenceEquals(this, other);

        public IEnumerator<long> GetEnumerator() => Enumerator;

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool IsEmpty => true;

        public bool Contains(long item) => false;

        public long[] ToArray() => EmptyArray;

        public override string ToString() => "[]";

        private class EmptyEnumerator : IEnumerator<long>
        {
            public bool MoveNext() => false;

            public void Reset() { }

            long IEnumerator<long>.Current => throw new NotSupportedException("Range is empty.");
            public object Current => throw new NotSupportedException("Range is empty.");

            public void Dispose() { }
        }
    }
}
