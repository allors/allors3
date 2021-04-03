// <copyright file="IMetaPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IMetaPopulation
    {
        IEnumerable<IDomain> Domains { get; }

        IEnumerable<IUnit> Units { get; }

        IEnumerable<IComposite> DatabaseComposites { get; }

        IEnumerable<IInterface> DatabaseInterfaces { get; }

        IEnumerable<IClass> DatabaseClasses { get; }

        IEnumerable<IRelationType> DatabaseRelationTypes { get; }

        IEnumerable<IMethodType> MethodTypes { get; }

        IMetaObject Find(Guid metaObjectId);

        IClass FindByName(string name);

        bool IsValid { get; }

        IValidationLog Validate();

        void Bind(Type[] types, MethodInfo[] methods);
    }
}
