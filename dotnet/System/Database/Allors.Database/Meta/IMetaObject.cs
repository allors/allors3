// <copyright file="IMetaObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    /// <summary>
    /// Base interface for Meta objects.
    /// </summary>
    public interface IMetaObject
    {
        IMetaPopulation MetaPopulation { get; }

        Origin Origin { get; }
    }
}
