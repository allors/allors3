// <copyright file="UntraceableCommands.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    public sealed class UntraceableCommands : Commands
    {
        public UntraceableCommands(Transaction transaction, IConnection connection) : base(transaction, connection)
        {
        }
    }
}
