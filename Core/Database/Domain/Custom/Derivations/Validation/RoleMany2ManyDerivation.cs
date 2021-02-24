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

    public class RoleMany2ManyDerivation : DomainDerivation
    {
        public RoleMany2ManyDerivation(M m) : base(m, new Guid("4383159F-258D-4FCB-833C-55D2B91109A1")) =>
            this.Patterns = new[]
            {
                new AssociationPattern(m.CC.Assigned) {Steps = new IPropertyType[]{m.CC.BBsWhereMany2Many, m.BB.AAsWhereMany2Many}},
                new AssociationPattern(m.CC.Assigned) {Steps = new IPropertyType[]{m.CC.BBsWhereUnusedMany2Many, m.BB.AAsWhereUnusedMany2Many}},
            };


        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                aa.Derived = aa.Many2Many.FirstOrDefault()?.Many2Many.FirstOrDefault()?.Assigned;
            }
        }
    }
}
