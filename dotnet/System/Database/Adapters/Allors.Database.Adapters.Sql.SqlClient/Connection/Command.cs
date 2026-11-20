// <copyright file="Command.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;

    using Meta;

    public class Command : ICommand
    {
        private readonly Connection connection;
        private readonly Mapping mapping;
        private readonly SqlCommand command;

        internal Command(Connection connection, Mapping mapping, SqlCommand command)
        {
            this.connection = connection;
            this.mapping = mapping;
            this.command = command;
        }

        public string CommandText
        {
            get => this.command.CommandText;

            set => this.command.CommandText = value;
        }

        public void Dispose() => this.command.Dispose();

        // SQL Server keeps the plan of a statement with parameters in a cache that every connection shares, so
        // this adapter has nothing to prepare.
        public void Prepare()
        {
        }

        public void AddInParameter(string parameterName, object value)
        {
            var parameter = this.command.Parameters.Contains(parameterName) ? this.command.Parameters[parameterName] : null;
            if (parameter == null)
            {
                parameter = this.command.CreateParameter();
                parameter.ParameterName = parameterName;
                if (value is DateTime)
                {
                    parameter.SqlDbType = SqlDbType.DateTime2;
                }

                this.command.Parameters.Add(parameter);
            }

            if (value == null || value == DBNull.Value)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }
        }

        public void ObjectParameter(long objectId) => this.GetOrCreateParameter(this.mapping.ParamInvocationNameForObject, Mapping.SqlDbTypeForObject).Value = objectId;

        public void AddTypeParameter(IClass @class) => this.GetOrCreateParameter(this.mapping.ParamInvocationNameForClass, Mapping.SqlDbTypeForClass).Value = @class.Id;

        public void AddCountParameter(int count) => this.GetOrCreateParameter(this.mapping.ParamNameForCount, Mapping.SqlDbTypeForCount).Value = count;

        public void AddUnitRoleParameter(IRoleType roleType, object unit) => this.GetOrCreateParameter(this.mapping.ParamNameByRoleType[roleType], this.mapping.GetSqlDbType(roleType)).Value = unit ?? DBNull.Value;

        public void AddCompositeRoleParameter(long objectId) => this.GetOrCreateParameter(this.mapping.ParamNameForCompositeRole, Mapping.SqlDbTypeForObject).Value = objectId;

        public void AddAssociationParameter(long objectId) => this.GetOrCreateParameter(this.mapping.ParamNameForAssociation, Mapping.SqlDbTypeForObject).Value = objectId;

        public void ObjectTableParameter(IEnumerable<long> objectIds) => this.GetOrCreateTableParameter(this.mapping.ParamNameForTableType, this.mapping.TableTypeNameForObject).Value = new ObjectDataRecord(this.mapping, objectIds);

        public void UnitTableParameter(IRoleType roleType, IEnumerable<UnitRelation> relations) => this.GetOrCreateTableParameter(this.mapping.ParamNameForTableType, this.mapping.GetTableTypeNameForRelation(roleType)).Value = new UnitRoleDataRecords(this.mapping, roleType, relations);

        public void AddCompositeRoleTableParameter(IEnumerable<CompositeRelation> relations) => this.GetOrCreateTableParameter(this.mapping.ParamNameForTableType, this.mapping.TableTypeNameForCompositeRelation).Value = new CompositeRoleDataRecords(this.mapping, relations);

        public void AddCompositesRoleTableParameter(IEnumerable<long> objectIds) => this.GetOrCreateTableParameter(this.mapping.ParamNameForTableType, this.mapping.TableTypeNameForObject).Value = new CompositesRoleDataRecords(this.mapping, objectIds);

        // The connection refuses a statement when its transaction has failed, and is told of a statement that
        // fails: here, and by the reader, where a statement can fail while its rows are read.
        public object ExecuteScalar()
        {
            this.connection.AssertNotFailed();

            try
            {
                return this.command.ExecuteScalar();
            }
            catch (Exception e)
            {
                this.connection.OnStatementFailed(e);
                throw;
            }
        }

        public void ExecuteNonQuery()
        {
            this.connection.AssertNotFailed();

            try
            {
                this.command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                this.connection.OnStatementFailed(e);
                throw;
            }
        }

        public IReader ExecuteReader()
        {
            this.connection.AssertNotFailed();

            try
            {
                return new Reader(this.connection, this.command.ExecuteReader());
            }
            catch (Exception e)
            {
                this.connection.OnStatementFailed(e);
                throw;
            }
        }

        public object GetValue(IReader reader, string tag, int i) =>
            tag switch
            {
                UnitTags.Binary => reader.GetValue(i),
                UnitTags.Boolean => reader.GetBoolean(i),
                UnitTags.DateTime => reader.GetDateTime(i),
                UnitTags.Decimal => reader.GetDecimal(i),
                UnitTags.Float => reader.GetDouble(i),
                UnitTags.Integer => reader.GetInt32(i),
                UnitTags.String => reader.GetString(i),
                UnitTags.Unique => reader.GetGuid(i),
                _ => throw new ArgumentException($"Unknown Unit Tag: {tag}")
            };

        private SqlParameter GetOrCreateParameter(string parameterName, SqlDbType dbType)
        {
            var parameter = this.command.Parameters.Contains(parameterName) ? this.command.Parameters[parameterName] : null;
            if (parameter != null)
            {
                return parameter;
            }

            parameter = this.command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.SqlDbType = dbType;
            this.command.Parameters.Add(parameter);

            return parameter;
        }

        private SqlParameter GetOrCreateTableParameter(string parameterName, string typeName)
        {
            var parameter = this.command.Parameters.Contains(parameterName) ? this.command.Parameters[parameterName] : null;
            if (parameter != null)
            {
                return parameter;
            }

            parameter = this.command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = typeName;
            this.command.Parameters.Add(parameter);

            return parameter;
        }
    }
}
