// <copyright file="Identity.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Threading;

    public abstract class Identity
    {
        public abstract long Id { get; }

        public override string ToString() => this.Id.ToString();
    }
}
