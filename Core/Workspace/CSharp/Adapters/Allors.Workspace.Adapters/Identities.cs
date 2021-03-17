// <copyright file="Id.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;

    public sealed class Identities
    {
        private long counter;

        public Identities() => this.counter = 0;

        public long NextId() => --this.counter;
    }
}
