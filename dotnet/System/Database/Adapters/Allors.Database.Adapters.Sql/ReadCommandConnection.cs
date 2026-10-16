// <copyright file="ReadCommandConnection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Meta;

    // Only adapter-owned commands receive this connection. No SQL parsing is involved.
    internal sealed class ReadCommandConnection : IConnection
    {
        private readonly IConnection connection;
        private readonly ReadGuard guard;

        internal ReadCommandConnection(IConnection connection, ReadGuard guard)
        {
            this.connection = connection;
            this.guard = guard;
        }

        public ICommand CreateCommand() => this.guard.Execute(() => (ICommand)new ReadCommand(this.connection.CreateCommand(), this.guard));

        public void Commit() => this.guard.Reject("Connection commit");

        public void Rollback() => this.guard.Reject("Connection rollback or restart");

        private sealed class ReadCommand : ICommand
        {
            private readonly ICommand command;
            private readonly ReadGuard guard;

            internal ReadCommand(ICommand command, ReadGuard guard)
            {
                this.command = command;
                this.guard = guard;
            }

            public CommandType CommandType { get => this.command.CommandType; set => this.command.CommandType = value; }

            public string CommandText { get => this.command.CommandText; set => this.command.CommandText = value; }

            public void AddInParameter(string name, object value) => this.command.AddInParameter(name, value);
            public void ObjectParameter(long id) => this.command.ObjectParameter(id);
            public void AddTypeParameter(IClass type) => this.command.AddTypeParameter(type);
            public void AddCountParameter(int count) => this.command.AddCountParameter(count);
            public void AddUnitRoleParameter(IRoleType roleType, object unit) => this.command.AddUnitRoleParameter(roleType, unit);
            public void AddCompositeRoleParameter(long id) => this.command.AddCompositeRoleParameter(id);
            public void AddAssociationParameter(long id) => this.command.AddAssociationParameter(id);
            public void AddCompositesRoleTableParameter(IEnumerable<long> ids) => this.command.AddCompositesRoleTableParameter(ids);
            public void ObjectTableParameter(IEnumerable<long> ids) => this.command.ObjectTableParameter(ids);
            public void UnitTableParameter(IRoleType roleType, IEnumerable<UnitRelation> relations) => this.command.UnitTableParameter(roleType, relations);
            public void AddCompositeRoleTableParameter(IEnumerable<CompositeRelation> relations) => this.command.AddCompositeRoleTableParameter(relations);

            public object ExecuteScalar() => this.guard.Execute(this.command.ExecuteScalar);

            public void ExecuteNonQuery() => this.guard.Reject("SQL mutation");

            public IReader ExecuteReader() => this.guard.Execute(() => (IReader)new ReadReader(this.command.ExecuteReader(), this.guard));

            public object GetValue(IReader reader, string tag, int i) => this.guard.Execute(() => this.command.GetValue(reader, tag, i));

            public void Dispose() => this.guard.Cleanup(this.command.Dispose);
        }

        private sealed class ReadReader : IReader
        {
            private readonly IReader reader;
            private readonly ReadGuard guard;

            internal ReadReader(IReader reader, ReadGuard guard)
            {
                this.reader = reader;
                this.guard = guard;
            }

            public bool Read() => this.guard.Execute(this.reader.Read);
            public object this[int i] => this.guard.Execute(() => this.reader[i]);
            public object GetValue(int i) => this.guard.Execute(() => this.reader.GetValue(i));
            public bool GetBoolean(int i) => this.guard.Execute(() => this.reader.GetBoolean(i));
            public DateTime GetDateTime(int i) => this.guard.Execute(() => this.reader.GetDateTime(i));
            public decimal GetDecimal(int i) => this.guard.Execute(() => this.reader.GetDecimal(i));
            public double GetDouble(int i) => this.guard.Execute(() => this.reader.GetDouble(i));
            public int GetInt32(int i) => this.guard.Execute(() => this.reader.GetInt32(i));
            public long GetInt64(int i) => this.guard.Execute(() => this.reader.GetInt64(i));
            public string GetString(int i) => this.guard.Execute(() => this.reader.GetString(i));
            public Guid GetGuid(int i) => this.guard.Execute(() => this.reader.GetGuid(i));
            public bool IsDBNull(int i) => this.guard.Execute(() => this.reader.IsDBNull(i));
            public void Dispose() => this.guard.Cleanup(this.reader.Dispose);
        }
    }
}
