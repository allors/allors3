// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class RoleMany2OneDerivation : DomainDerivation
    {
        public RoleMany2OneDerivation(M m) : base(m, new Guid("cbebe35e-9931-4701-8b05-8ed61b266bb2")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(m.CC.Assigned) {Steps = new IPropertyType[]{m.CC.BBsWhereMany2One, m.BB.AAsWhereMany2One}},
                new ChangedPattern(m.CC.Assigned) {Steps = new IPropertyType[]{m.CC.BBsWhereUnusedMany2One, m.BB.AAsWhereUnusedMany2One}},
            };


        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                aa.Derived = aa.Many2One?.Many2One?.Assigned;
            }
        }
    }
}
