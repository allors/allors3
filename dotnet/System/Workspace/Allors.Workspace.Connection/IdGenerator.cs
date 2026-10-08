// <copyright file="IdGenerator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    /// <summary>
    /// Gives new objects their workspace ids: negative, counting down from -1. A database id is
    /// positive. A workspace owns a generator; the connection meets its ids in a push of new
    /// objects and answers with the database ids.
    /// </summary>
    public sealed class IdGenerator
    {
        private long counter;

        public IdGenerator() => this.counter = 0;

        public long Next() => --this.counter;
    }
}
