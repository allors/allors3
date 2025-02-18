// <copyright file="IPreparedExtents.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Services
{
    using Meta;

    public interface IObjectBuilderService
    {
        IObject Build(IClass @class);
    }
}
