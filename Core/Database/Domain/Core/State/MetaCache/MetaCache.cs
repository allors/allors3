// <copyright file="MetaCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class MetaCache : IMetaCache
    {
        private readonly IReadOnlyDictionary<IClass, RoleType[]> requiredRoleTypesByClass;
        private readonly IReadOnlyDictionary<IClass, RoleType[]> uniqueRoleTypesByClass;
        private readonly IReadOnlyDictionary<IClass, Type> builderTypeByClass;

        public MetaCache(IDatabaseState databaseState)
        {
            this.DatabaseState = databaseState;

            var database = this.DatabaseState.Database;
            var metaPopulation = database.MetaPopulation;
            var assembly = database.ObjectFactory.Assembly;

            this.requiredRoleTypesByClass = metaPopulation.DatabaseClasses
                .ToDictionary(
                    v => v,
                    v => ((Class)v).RoleTypes
                          .Where(concreteRoleType => concreteRoleType.IsRequired)
                          .ToArray());


            this.uniqueRoleTypesByClass = metaPopulation.DatabaseClasses
                .ToDictionary(
                    v => v,
                    v => ((Class)v).RoleTypes
                        .Where(concreteRoleType => concreteRoleType.IsUnique)
                        .ToArray());

            this.builderTypeByClass = metaPopulation.DatabaseClasses.
                ToDictionary(
                    v => v,
                    v => assembly.GetType($"Allors.Domain.{v.Name}Builder", false));

        }

        public IDatabaseState DatabaseState { get; }

        public RoleType[] GetRequiredRoleTypes(IClass @class) => this.requiredRoleTypesByClass[@class];

        public RoleType[] GetUniqueRoleTypes(IClass @class) => this.uniqueRoleTypesByClass[@class];

        public Type GetBuilderType(IClass @class) => this.builderTypeByClass[@class];
    }
}