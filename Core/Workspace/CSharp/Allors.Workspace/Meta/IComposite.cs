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

        bool ExistDatabaseClass { get; }

        IEnumerable<IClass> DatabaseClasses { get; }

        bool ExistExclusiveDatabaseClass { get; }

        IClass ExclusiveDatabaseClass { get; }

        bool IsSynced { get; }

        // TODO: change to HasSuperType
        bool ExistSupertype(IInterface @interface);

        bool ExistAssociationType(IAssociationType association);

        bool ExistRoleType(IRoleType roleType);

        bool IsAssignableFrom(IComposite objectType);
    }
}
