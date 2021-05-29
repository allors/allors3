// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public static partial class Rules
    {
        public static Rule[] Create(MetaPopulation m) =>
            new Rule[]
            {
                // Custom
                new PersonFullNameRule(m),
                new PersonGreetingRule(m),
                new PersonOwningRule(m),
                new OrganisationJustDidItRule(m),

                // Validation
                new RoleOne2OneRule(m),
                new RoleOne2ManyRule(m),
                new RoleMany2OneRule(m),
                new RoleMany2ManyRule(m),

                // RoleTypeHierarchy
                new C1ChangedRoleRule(m),
                new I12ChangedRoleRule(m),
                new I1ChangedRoleRule(m),
                new S12ChangedRoleRule(m),
            };
    }
}
