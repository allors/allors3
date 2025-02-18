// <copyright file="IDomainDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Workspace.Derivations
{
    using System.Collections.Generic;
    using Meta;

    public interface IRule
    {
        IComposite ObjectType { get; }

        IRoleType RoleType { get; }

        IEnumerable<IDependency> Dependencies { get; }

        object Derive(IObject match);
    }
}
