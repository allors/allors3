// <copyright file="RoleChange.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Meta;

    /// <summary>
    /// The new value of a role, in the shape a record holds it: a unit, the id of a composite
    /// role as a long, or the ids of a composites role as an enumerable of long; null removes
    /// the role.
    /// </summary>
    public sealed class RoleChange
    {
        public RoleChange(IRoleType roleType, object value)
        {
            this.RoleType = roleType;
            this.Value = value;
        }

        public IRoleType RoleType { get; }

        public object Value { get; }
    }
}
