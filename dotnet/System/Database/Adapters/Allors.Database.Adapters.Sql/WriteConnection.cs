// <copyright file="WriteConnection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    internal sealed class WriteConnection : IConnection
    {
        private readonly WriteScope scope;

        internal WriteConnection(WriteScope scope) => this.scope = scope;

        public ICommand CreateCommand()
        {
            this.scope.Reject("Direct SQL command creation");
            return null;
        }

        public void Commit() => this.scope.Reject("Direct connection commit");

        public void Rollback() => this.scope.Reject("Direct connection rollback");
    }
}
