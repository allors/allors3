// <copyright file="Identity.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;

    public abstract class Identity : IComparable<Identity>
    {
        public abstract long Id { get; }

        public abstract long CompareId { get; }

        public int CompareTo(Identity other) => this.CompareId.CompareTo(other.CompareId);

        public override string ToString() => this.Id.ToString();
    }
}
