// <copyright file="IObjects.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public interface IObjects : ISetup, ISecure
    {
        ITransaction Transaction { get; }

        Composite ObjectType { get; }
    }
}
