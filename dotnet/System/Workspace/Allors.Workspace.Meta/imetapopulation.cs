// <copyright file="IMetaPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;

    public interface IMetaPopulation
    {
        IEnumerable<IUnit> Units { get; }

        IEnumerable<IInterface> Interfaces { get; }

        IEnumerable<IClass> Classes { get; }

        IEnumerable<IRelationType> RelationTypes { get; }

        IEnumerable<IMethodType> MethodTypes { get; }

        IEnumerable<IComposite> Composites { get; }

        IMetaObject FindByTag(string tag);

        IComposite FindByName(string name);

        void Bind(Type[] types);
    }
}
