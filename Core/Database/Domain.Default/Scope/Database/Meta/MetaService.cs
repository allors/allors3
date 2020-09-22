// <copyright file="TreeService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Meta;

    public class MetaService : IMetaService
    {
        private readonly IReadOnlyDictionary<IClass, RoleType[]> requiredRoleTypesByClass;
        private readonly IReadOnlyDictionary<IClass, RoleType[]> uniqueRoleTypesByClass;
        private readonly IReadOnlyDictionary<IClass, Type> builderTypeByClass;

        public MetaService(IMetaPopulation metaPopulation, Assembly assembly)
        {
            this.requiredRoleTypesByClass = metaPopulation.DatabaseClasses
                .ToDictionary(
                    v => v,
                    v => ((Class)v).RoleClasses
                          .Where(concreteRoleType => concreteRoleType.IsRequired)
                          .Select(concreteRoleType => concreteRoleType.RoleType)
                          .ToArray());


            this.uniqueRoleTypesByClass = metaPopulation.DatabaseClasses
                .ToDictionary(
                    v => v,
                    v => ((Class)v).RoleClasses
                        .Where(concreteRoleType => concreteRoleType.IsUnique)
                        .Select(concreteRoleType => concreteRoleType.RoleType)
                        .ToArray());

            this.builderTypeByClass = metaPopulation.DatabaseClasses.
                ToDictionary(
                    v => v,
                    v => assembly.GetType($"Allors.Domain.{v.Name}Builder", false));

        }

        public RoleType[] GetRequiredRoleTypes(IClass @class) => this.requiredRoleTypesByClass[@class];

        public RoleType[] GetUniqueRoleTypes(IClass @class) => this.uniqueRoleTypesByClass[@class];

        public Type GetBuilderType(IClass @class) => this.builderTypeByClass[@class];
    }
}
