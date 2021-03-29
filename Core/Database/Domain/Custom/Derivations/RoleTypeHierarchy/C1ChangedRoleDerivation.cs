// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class C1ChangedRoleDerivation : Rule
    {
        public C1ChangedRoleDerivation(M m) : base(m, new Guid("84343F1E-7224-41CE-9B4C-69883417115F")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.C1, this.M.C1.ChangedRolePing)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var c1 in matches.Cast<C1>())
            {
                c1.ChangedRolePong = c1.ChangedRolePing;
            }
        }
    }
}
