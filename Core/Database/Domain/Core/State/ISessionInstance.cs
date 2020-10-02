// <copyright file="ISessionInstance.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Domain;

    public partial interface ISessionInstance : ISessionInstanceLifecycle
    {
        User User { get; set; }
    }
}
