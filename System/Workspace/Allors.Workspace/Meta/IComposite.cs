// <copyright file="IComposite.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System.Collections.Generic;

    public interface IComposite : IObjectType
    {
        IEnumerable<IAssociationType> DatabaseAssociationTypes { get; }

        IEnumerable<IRoleType> DatabaseRoleTypes { get; }

        IEnumerable<IRoleType> WorkspaceRoleTypes { get; }

        IEnumerable<IClass> DatabaseClasses { get; }

        bool IsAssignableFrom(IComposite objectType);
    }
}
