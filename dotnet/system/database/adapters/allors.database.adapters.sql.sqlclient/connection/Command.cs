// <copyright file="Command.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
        private ParameterCollection parameters;

        internal Command(Mapping mapping, SqlCommand command)
        {
            this.Mapping = mapping;
            this.SqlCommand = command;
            this.parameters = new ParameterCollection(this.SqlCommand.Parameters);
        }

        public ISqlParameterCollection Parameters => this.parameters;

        public CommandType CommandType
        {
            get => this.SqlCommand.CommandType;

            set => this.SqlCommand.CommandType = value;
        }

        public string CommandText
        {
            get => this.SqlCommand.CommandText;

            set => this.SqlCommand.CommandText = value;
        }

        protected Mapping Mapping { get; }

        protected SqlCommand SqlCommand { get; }

        public void Dispose() => this.SqlCommand.Dispose();

        public ISqlParameter CreateParameter() => new Parameter(this.SqlCommand.CreateParameter());
        
        public void AddInParameter(string parameterName, object value)
        {
            var sqlParameter = this.SqlCommand.Parameters.Contains(parameterName) ? this.SqlCommand.Parameters[parameterName] : null;
            if (sqlParameter == null)
            {
                sqlParameter = this.SqlCommand.CreateParameter();
                sqlParameter.ParameterName = parameterName;
                if (value is DateTime)
                {
                    sqlParameter.SqlDbType = SqlDbType.DateTime2;
                }

                this.SqlCommand.Parameters.Add(sqlParameter);
            }

            if (value == null || value == DBNull.Value)
            {
                this.SqlCommand.Parameters[parameterName].Value = DBNull.Value;
            }
            else
            {
                this.SqlCommand.Parameters[parameterName].Value = value;
            }
        }

        public void AddObjectParameter(long objectId)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForObject;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.SqlCommand.Parameters.Add(sqlParameter);
        }

        public void AddTypeParameter(IClass @class)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForClass;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForClass;
            sqlParameter.Value = @class.Id;

            this.parameters.Add(sqlParameter);
        }

        public void AddCountParameter(int count)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForCount;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForCount;
            sqlParameter.Value = count;

            this.parameters.Add(sqlParameter);
        }

        public void AddUnitRoleParameter(IRoleType roleType, object unit)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = this.Mapping.ParamNameByRoleType[roleType];
            sqlParameter.SqlDbType = this.Mapping.GetSqlDbType(roleType);
            sqlParameter.Value = unit ?? DBNull.Value;

            this.parameters.Add(sqlParameter);
        }

        public void AddUnitTableParameter(IRoleType roleType, IEnumerable<UnitRelation> relations)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = this.Mapping.GetTableTypeName(roleType);
            sqlParameter.ParameterName = Mapping.ParamNameForTableType;
            sqlParameter.Value = new UnitRoleDataRecords(this.Mapping, roleType, relations);

            this.parameters.Add(sqlParameter);
        }

        public void AddCompositeRoleParameter(long objectId)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForCompositeRole;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.parameters.Add(sqlParameter);
        }

        public void AddAssociationParameter(long objectId)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForAssociation;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.parameters.Add(sqlParameter);
        }

        public void AddObjectTableParameter(IEnumerable<Reference> references)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = this.Mapping.TableTypeNameForObject;
            sqlParameter.ParameterName = Mapping.ParamNameForTableType;
            sqlParameter.Value = new CompositesRoleDataRecords(this.Mapping, references);

            this.parameters.Add(sqlParameter);
        }

        public void AddObjectTableParameter(IEnumerable<long> objectIds)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = this.Mapping.TableTypeNameForObject;
            sqlParameter.ParameterName = Mapping.ParamNameForTableType;
            sqlParameter.Value = new ObjectDataRecord(this.Mapping, objectIds);

            this.parameters.Add(sqlParameter);
        }

        public void AddCompositeRoleTableParameter(IEnumerable<CompositeRelation> relations)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = this.Mapping.TableTypeNameForCompositeRelation;
            sqlParameter.ParameterName = Mapping.ParamNameForTableType;
            sqlParameter.Value = new CompositeRoleDataRecords(this.Mapping, relations);

            this.parameters.Add(sqlParameter);
        }

        public void AddAssociationTableParameter(long objectId)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForAssociation;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.parameters.Add(sqlParameter);
        }

        public object ExecuteScalar() => this.SqlCommand.ExecuteScalar();

        public void ExecuteNonQuery() => this.SqlCommand.ExecuteNonQuery();

        public IReader ExecuteReader() => new Reader(this.SqlCommand.ExecuteReader());

        public object GetValue(IReader reader, int tag, int i) =>
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
                _ => throw new ArgumentException("Unknown Unit Tag: " + tag)
            };
    }
}
