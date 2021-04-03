// <copyright file="IMetaPopulationBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System.Collections.Generic;

    public partial interface IMetaPopulationBase : IMetaPopulation
    {
        new IEnumerable<IDomainBase> Domains { get; }

        new IEnumerable<IUnitBase> Units { get; }

        new IEnumerable<ICompositeBase> DatabaseComposites { get; }

        new IEnumerable<IInterfaceBase> DatabaseInterfaces { get; }

        new IEnumerable<IClassBase> DatabaseClasses { get; }

        new IEnumerable<IRelationTypeBase> DatabaseRelationTypes { get; }

        new IEnumerable<IMethodTypeBase> MethodTypes { get; }

        new IEnumerable<IInheritanceBase> Inheritances { get; }

        void OnDomainCreated(Domain domain);

        void OnInterfaceCreated(Interface @interface);

        void OnClassCreated(Class @class);

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
