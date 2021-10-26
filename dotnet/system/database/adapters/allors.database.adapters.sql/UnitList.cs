// <copyright file="Transaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Transaction type.</summary>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections.Generic;
    using Meta;

    public sealed class UnitList
    {
        public IRoleType RoleType { get; }

        public IEnumerable<object> Values { get; }

        public UnitList(IRoleType roleType, IEnumerable<object> values)
        {
            this.RoleType = roleType;
            this.Values = values;
        }
    }
}
