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

    public class RoleOne2ManyRule : Rule
    {
        public RoleOne2ManyRule(MetaPopulation m) : base(m, new Guid("d40ab5c5-c248-4455-bad4-8c825f48e080")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.CC, m.CC.Assigned) {Steps = new IPropertyType[]{m.CC.BBWhereOne2Many, m.BB.AAWhereOne2Many}},
                new RolePattern(m.CC, m.CC.Assigned) {Steps = new IPropertyType[]{m.CC.BBWhereUnusedOne2Many, m.BB.AAWhereUnusedOne2Many}},
            };


        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var aa in matches.Cast<AA>())
            {
                aa.Derived = aa.One2Many.FirstOrDefault()?.One2Many.FirstOrDefault()?.Assigned;
            }
        }
    }
}
