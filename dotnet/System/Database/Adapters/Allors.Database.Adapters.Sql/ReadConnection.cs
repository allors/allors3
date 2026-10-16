// <copyright file="ReadConnection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    // The public facade never hands arbitrary SQL to the protected connection.
    internal sealed class ReadConnection : IConnection
    {
        private readonly ReadGuard guard;

        internal ReadConnection(ReadGuard guard) => this.guard = guard;

        public ICommand CreateCommand()
        {
            this.guard.Reject("Direct SQL command creation");
            return null;
        }

        public void Commit() => this.guard.Reject("Connection commit");

        public void Rollback() => this.guard.Reject("Connection rollback or restart");
    }
}
