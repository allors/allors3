// <copyright file="UntraceableCommands.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
