// <copyright file="SessionScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Domain;
    using Services;

    public abstract partial class SessionScope : ISessionScope
    {
        public abstract void OnInit(ISession session);

        public User User { get; set; }
    }
}
