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

        IEnumerable<IInheritanceBase> Inheritances { get; }

        MethodCompiler MethodCompiler { get; }

        void OnDomainCreated(Domain domain);

        void OnInheritanceCreated(Inheritance inheritance);

        void OnInterfaceCreated(Interface @interface);

        void OnClassCreated(Class @class);

        void OnMethodTypeCreated(MethodType methodType);

        void OnRelationTypeCreated(RelationType relationType);

        void OnAssociationTypeCreated(AssociationType associationType);

        void OnRoleTypeCreated(RoleType roleType);

        void Stale();

        void AssertUnlocked();

        void Derive();
    }
}
