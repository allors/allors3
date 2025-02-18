// <copyright file="WorkspaceIdGenerator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    public sealed class IdGenerator
    {
        private long counter;

        public IdGenerator() => this.counter = 0;

        public long Next() => --this.counter;
    }
}
