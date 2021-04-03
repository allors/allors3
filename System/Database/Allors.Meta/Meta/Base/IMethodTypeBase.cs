// <copyright file="IClassBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public partial interface IMethodTypeBase : IMethodType
    {
        ICompositeBase Composite { get; }

        void Validate(ValidationLog log);

        void DeriveMethodClasses();

        IMethodClassBase MethodClassBy(IClassBase @class);

        void DeriveWorkspaceNames();
    }
}
