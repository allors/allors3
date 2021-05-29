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

    public class AccessControlEffectiveUsersRule : Rule
    {
        public AccessControlEffectiveUsersRule(MetaPopulation m) : base(m, new Guid("2D3F4F02-7439-48E7-9E5B-363F4A4384F0")) =>
            this.Patterns = new Pattern[]
            {
                m.AccessControl.RolePattern(v=>v.Subjects),
                m.AccessControl.RolePattern(v=>v.SubjectGroups),
                m.UserGroup.RolePattern(v=>v.Members, v=>v.AccessControlsWhereSubjectGroup),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var accessControl in matches.Cast<AccessControl>())
            {
                accessControl.EffectiveUsers = accessControl
                    .SubjectGroups.SelectMany(v => v.Members)
                    .Union(accessControl.Subjects)
                    .ToArray();

                // Invalidate cache
                accessControl.DatabaseServices().AccessControlCache.Clear(accessControl.Id);
            }
        }
    }
}
