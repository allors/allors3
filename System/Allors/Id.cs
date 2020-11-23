// <copyright file="Multiplicity.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;

    public readonly struct Id : IEquatable<Id>
    {
        public static readonly Id None = 0;

        public static readonly Id Initial = 1;

        public Id(long value) => this.Value = value;

        public long Value { get; }

        public bool HasValue => this != None;

        public bool IsInitial => this == Initial;

        public static bool operator ==(Id @this, Id other) => @this.Equals(other);

        public static bool operator !=(Id @this, Id other) => !@this.Equals(other);

        public static implicit operator long(Id id) => id.Value;

        public static implicit operator Id(long value) => new Id(value);

        public bool Equals(Id other) => this.Value == other.Value;

        public override bool Equals(object obj) => obj is Id other && this.Equals(other);

        public override int GetHashCode() => this.Value.GetHashCode();
    }
}
