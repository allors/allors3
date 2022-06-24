// <copyright file="PullPrefetchers.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using Meta;

    public interface IPullPrefetchers
    {
        PrefetchPolicy ForDependency(IComposite composite, ISet<IPropertyType> propertyTypes);
    }
}
