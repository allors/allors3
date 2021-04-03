// <copyright file="IMethodClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;

    public interface IMethodClass : IMethodType
    {
        new IClass ObjectType { get; }

        IMethodInterface MethodInterface { get; }

        IEnumerable<Action<object, object>> Actions { get; }
    }
}
