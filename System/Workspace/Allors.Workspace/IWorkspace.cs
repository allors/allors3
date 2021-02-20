// <copyright file="IWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;
    using System.Collections.Generic;
    using Derivations;
    using Meta;

    public interface IWorkspace
    {
        IMetaPopulation MetaPopulation { get; }

        IObjectFactory ObjectFactory { get; }

        IWorkspaceLifecycle StateLifecycle { get; }

        IEnumerable<ISession> Sessions { get; }

        ISession CreateSession();

        IChangeSet[] Checkpoint();

        IDictionary<Guid, IDomainDerivation> DomainDerivationById { get; }
    }
}