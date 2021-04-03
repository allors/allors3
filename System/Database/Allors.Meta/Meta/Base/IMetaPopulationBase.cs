// <copyright file="IMetaPopulationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System.Collections.Generic;

    public partial interface IMetaPopulationBase : IMetaPopulation
    {
        string[] WorkspaceNames { get; }

        IEnumerable<Inheritance> Inheritances { get; }

        void OnDomainCreated(Domain domain);

        void OnMethodInterfaceCreated(MethodInterface methodInterface);

        void OnRelationTypeCreated(RelationType relationType);

        void OnAssociationTypeCreated(AssociationType associationType);

        void OnRoleTypeCreated(RoleType roleType);

        void OnMethodClassCreated(MethodClass methodClass);

        void Stale();

        void AssertUnlocked();

        void Derive();
    }
}
