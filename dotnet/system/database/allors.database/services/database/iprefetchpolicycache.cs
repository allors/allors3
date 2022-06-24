// <copyright file="IPreparedSelects.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Data;
    using Meta;

    public interface IPrefetchPolicyCache
    {
        PrefetchPolicy PermissionsWithClass { get; }

        PrefetchPolicy Security { get; }

        PrefetchPolicy ForDependency(IComposite composite, ISet<IPropertyType> propertyTypes);

        IDictionary<IClass, PrefetchPolicy> WorkspacePrefetchPolicyByClass(string workspaceName);

        PrefetchPolicy ForNodes(Node[] nodes);
    }
}
