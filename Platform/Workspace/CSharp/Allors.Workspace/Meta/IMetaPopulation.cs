// <copyright file="IMetaPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IMetaPopulation
    {
        IEnumerable<IComposite> DatabaseComposites { get; }

        IEnumerable<IClass> DatabaseClasses { get; }

        IEnumerable<IRelationType> DatabaseRelationTypes { get; }

        IMetaObject Find(Guid metaObjectId);

        bool IsValid { get; }

        IValidationLog Validate();

        void Bind(Type[] types, MethodInfo[] methods);
    }
}
