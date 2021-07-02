// <copyright file="Command.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Meta;
    using global::Npgsql;
    using NpgsqlTypes;

    public class Command : ICommand
    {
        internal Command(Mapping mapping, NpgsqlCommand command)
        {
            this.Mapping = mapping;
            this.NpgsqlCommand = command;
        }

        internal NpgsqlParameterCollection Parameters => this.NpgsqlCommand.Parameters;

        public CommandType CommandType
        {
            get => this.NpgsqlCommand.CommandType;
            set => this.NpgsqlCommand.CommandType = value;
        }

        public string CommandText
        {
            get => this.NpgsqlCommand.CommandText;
            set => this.NpgsqlCommand.CommandText = value;
        }

        public Mapping Mapping { get; }

        public NpgsqlCommand NpgsqlCommand { get; }

        public void Dispose() => this.NpgsqlCommand.Dispose();

        public NpgsqlParameter CreateParameter() => this.NpgsqlCommand.CreateParameter();

        public NpgsqlParameter GetParameter(string name) => this.NpgsqlCommand.Parameters[name];

        public void AddInParameter(string parameterName, object value)
        {
            var sqlParameter = this.NpgsqlCommand.Parameters.Contains(parameterName) ? this.NpgsqlCommand.Parameters[parameterName] : null;
            if (sqlParameter == null)
            {
                sqlParameter = this.NpgsqlCommand.CreateParameter();
                sqlParameter.ParameterName = parameterName;
                if (value is DateTime)
                {
                    sqlParameter.NpgsqlDbType = NpgsqlDbType.Timestamp;
                }

                this.NpgsqlCommand.Parameters.Add(sqlParameter);
            }

            if (value == null || value == DBNull.Value)
            {
                this.NpgsqlCommand.Parameters[parameterName].Value = DBNull.Value;
            }
            else
            {
                this.NpgsqlCommand.Parameters[parameterName].Value = value;
            }
        }

        public void ObjectParameter(long objectId)
        {
            var sqlParameter = this.NpgsqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForObject;
            sqlParameter.NpgsqlDbType = Mapping.NpgsqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.NpgsqlCommand.Parameters.Add(sqlParameter);
        }

        public void AddTypeParameter(IClass @class)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForClass;
            sqlParameter.NpgsqlDbType = Mapping.NpgsqlDbTypeForClass;
            sqlParameter.Value = @class.Id;

            this.Parameters.Add(sqlParameter);
        }

        public void AddCountParameter(int count)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForCount;
            sqlParameter.NpgsqlDbType = Mapping.NpgsqlDbTypeForCount;
            sqlParameter.Value = count;

            this.Parameters.Add(sqlParameter);
        }

        public void AddCompositeRoleParameter(long objectId)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForCompositeRole;
            sqlParameter.NpgsqlDbType = Mapping.NpgsqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.Parameters.Add(sqlParameter);
        }

        public void AddAssociationParameter(long objectId)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForAssociation;
            sqlParameter.NpgsqlDbType = Mapping.NpgsqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.Parameters.Add(sqlParameter);
        }

        public void ObjectTableParameter(IEnumerable<long> objectIds)
        {
            var objectParameter = this.GetOrCreateTableParameter(this.Mapping.ObjectArrayParam.InvocationName, Mapping.NpgsqlDbTypeForObject);
            objectParameter.Value = objectIds;
        }

        public void UnitTableParameter(IRoleType roleType, ICollection<UnitRelation> relations)
        {
            var objectParameter = this.GetOrCreateTableParameter(this.Mapping.ObjectArrayParam.InvocationName, Mapping.NpgsqlDbTypeForObject);
            var roleParameter = this.GetOrCreateTableParameter(this.Mapping.StringRoleArrayParam.InvocationName, this.Mapping.GetNpgsqlDbType(roleType));

            objectParameter.Value = relations.Select(v => v.Association).ToArray();
            roleParameter.Value = relations.Select(v => v.Role).ToArray();
        }

        public void AddCompositeRoleTableParameter(ICollection<CompositeRelation> relations)
        {
            var objectParameter = this.GetParameter(this.Mapping.ObjectArrayParam.InvocationName);
            var roleParameter = this.GetParameter(this.Mapping.StringRoleArrayParam.InvocationName); // TODO: should be a shared name

            objectParameter.Value = relations.Select(v => v.Association).ToArray();
            roleParameter.Value = relations.Select(v => v.Role).ToArray();
        }

        public void SetCompositeRoleArrayParameter(ICollection<CompositeRelation> relations)
        {
            var objectParameter = this.GetParameter(this.Mapping.ObjectArrayParam.InvocationName);
            var roleParameter = this.GetParameter(this.Mapping.StringRoleArrayParam.InvocationName); // TODO: should be a shared name

            objectParameter.Value = relations.Select(v => v.Association).ToArray();
            roleParameter.Value = relations.Select(v => v.Role).ToArray();
        }

        public object ExecuteScalar() => this.NpgsqlCommand.ExecuteScalar();

        public void ExecuteNonQuery() => this.NpgsqlCommand.ExecuteNonQuery();

        public IReader ExecuteReader() => new Reader(this.NpgsqlCommand.ExecuteReader());

        public object GetValue(IReader reader, int tag, int i) =>
            tag switch
            {
                UnitTags.String => reader.GetString(i),
                UnitTags.Integer => reader.GetInt32(i),
                UnitTags.Float => reader.GetDouble(i),
                UnitTags.Decimal => reader.GetDecimal(i),
                UnitTags.Boolean => reader.GetBoolean(i),
                UnitTags.DateTime => reader.GetDateTime(i),
                UnitTags.Unique => reader.GetGuid(i),
                UnitTags.Binary => reader.GetValue(i),
                _ => throw new ArgumentException("Unknown Unit Tag: " + tag)
            };

        private NpgsqlParameter GetOrCreateTableParameter(string parameterName, NpgsqlDbType type)
        {
            var objectParameter = this.GetParameter(parameterName);
            if (objectParameter != null)
            {
                return objectParameter;
            }

            objectParameter = this.CreateParameter();
            objectParameter.NpgsqlDbType = NpgsqlDbType.Array | type;
            objectParameter.ParameterName = parameterName;
            this.Parameters.Add(objectParameter);
            return objectParameter;
        }
    }
}
